using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RBBot.Core.Engine;
using System.Net.WebSockets;
using System.Threading;
using Newtonsoft.Json;
using RBBot.Core.Models;
using RBBot.Core.Engine.Trading;
using Gdax;
using Gdax.Accounts;
using Gdax.Authentication;
using Gdax.CommonModels;


namespace RBBot.Core.Exchanges.GDAX
{
    public class GDAXIntegration : ExchangeIntegration, IExchangeTrader
    {
        
        public override string Name { get { return "GDAX"; } }

        private ClientWebSocket websocket = null;

        public GDAXIntegration(IMarketPriceObserver[] priceObservers, Exchange[] exchanges) : base(priceObservers, exchanges)
        {
        }

        public override async Task InitializeExchangePriceProcessingAsync()
        {
            // Get all the products from the trading pairs.
            string products = string.Join(",", this.tradingPairs.ToList().Select(x => "\"" + x.Value.TradePair.FromCurrency.Code + "-" + x.Value.TradePair.ToCurrency.Code + "\""));

            // Form the initiation string
            var text = @"{ ""type"": ""subscribe"", ""product_ids"": [" + products + "] }";

#warning    This is still all hardcoded here!
            var uri = "wss://ws-feed.gdax.com";

            // And initialize the websocket, pasing ParseResult as the callback to parse data from GDAX
            this.websocket = await Helpers.WebSocketManager.Initialize(uri, new[] { text }, ParseResult);

        }

        public override async Task ShutdownExchangePriceProcessingDownAsync()
        {
            await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }



        // Callback when gdax send information.
        private async Task ParseResult(string result)
        {
            try
            {


                // We just want matches. Discard everything else.
                if (result.Contains("match"))
                {
                    var jsonObj = JsonConvert.DeserializeObject<GDAXTradeMatchJson>(result);



                    PriceChangeEvent priceChange = new PriceChangeEvent()
                    {
                        Price = jsonObj.price,
                        UtcTime = DateTime.Parse(jsonObj.time).ToUniversalTime(),
                        ExchangeTradePair = this.GetExchangeTradePair(this.tradingPairs.Values.First().Exchange.Name, jsonObj.product_id.Split('-')[0], jsonObj.product_id.Split('-')[1])

                    };

                    // Notify the observers!
                    await this.NotifyObserverOfPriceChange(priceChange);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        public Task<ExchangeBalance[]> GetBalancesAsync()
        {
            throw new NotImplementedException();
            //GdaxClient c = new GdaxClient();
            //GdaxAuthenticator au = new GdaxAuthenticator()
            //throw new NotImplementedException();
        }

        public Task<ExchangeBalance> GetBalanceAsync(Currency currency)
        {
            throw new NotImplementedException();
        }

        public Task DepositAsync(Currency currency, decimal amount, string fromAccountAddress, string toAccountAddress)
        {
            throw new NotImplementedException();
        }

        public Task WithdrawAsync(Currency currency, decimal amount, string fromAccountAddress, string toAccountAddress)
        {
            throw new NotImplementedException();
        }

        public Task PlaceOrder()
        {
            throw new NotImplementedException();
        }
    }
}
