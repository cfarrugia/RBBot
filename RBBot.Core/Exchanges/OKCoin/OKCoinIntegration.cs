using Newtonsoft.Json.Linq;
using RBBot.Core.Engine;
using RBBot.Core.Engine.Trading;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges.OKCoin
{
    /// <summary>
    /// Took me some time to realize this but:
    /// OKCoin.com allows BTC - USD and ETH - USD
    /// OKCoin.cn allows BTC - CNY and ETH - CNY
    /// OKEx allows ETH - BTC and other altcoin trades
    /// </summary>
    public abstract class OKCoinIntegration : ExchangeIntegration
    {
        public OKCoinIntegration(IMarketPriceProcessor[] priceObservers, Exchange[] exchange) : base(priceObservers, exchange)
        {
        }


#warning Need to put in db and secure!
        public abstract string wssUri { get; }
        
        public const string apiKey = "1da2ac66-a2b9-4c69-92ef-50dcf7d9b3ed";
        public const string secretKey =  "EF702AFB7B74DD203ED2BC1DCB7250B9";

        
        private ClientWebSocket websocket = null;


        private Dictionary<string, ExchangeTradePair> subsribedProducts { get; set; }


        public override async Task InitializeExchangePriceProcessingAsync()
        {
            // Get all the products from the trading pairs.
            subsribedProducts = this.tradingPairs.ToDictionary(x => "ok_sub_spot" + x.Value.TradePair.ToCurrency.Code.ToLower() + "_" + x.Value.TradePair.FromCurrency.Code.ToLower() + "_ticker", y => y.Value);

            // 
            string[] subscriptions = subsribedProducts.Keys.Select(x => @"{ ""event"":""addChannel"",""channel"":""" + x + @"""}").ToArray();

            // And initialize the websocket, pasing ParseResult as the callback to parse data from GDAX
            this.websocket = await Helpers.WebSocketManager.Initialize(wssUri, subscriptions , ParseResult);
        }

        public override async Task ShutdownExchangePriceProcessingDownAsync()
        {
            await this.websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }

        private async Task ParseResult(string result)
        {
            try
            {

                // Get JSON array and extract product, time and last trade price.
                dynamic dynObj = JArray.Parse(result);

                // If the dynamic object contains a result object inside, then this is just an "ok" on successfull connection.
                if (dynObj[0].data.result != null) return;


                dynamic dynData = dynObj;

                string productName = dynData[0].channel.Value;
                long timestamp = dynData[0].data.timestamp.Value;
                double lasttradeprice = dynData[0].data.last.Value;

                await this.NotifyObserverOfPriceChange(new PriceChangeEvent()
                {
                    ExchangeTradePair = this.subsribedProducts[productName],
                    Price = (decimal)lasttradeprice,
                    UtcTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime // It appears they use unix time.
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
