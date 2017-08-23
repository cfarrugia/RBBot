using Newtonsoft.Json;
using RBBot.Core.Engine;
using RBBot.Core.Engine.MarketObservers;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges.Poloniex
{
    public class PoloniexIntegration : ExchangeIntegration
    {
        public override string Name {  get { return "Poloniex";  } }


        private ClientWebSocket websocket = null;


        public PoloniexIntegration(IMarketPriceObserver[] priceObservers, Exchange[] exchanges) : base(priceObservers, exchanges)
        {
        }

        public override async Task InitializeAsync()
        {
            // Get all the products from the trading pairs.
            // For some reason the from and to currencies are the other way round. Mahh. 
            //List<string> messages = this.tradingPairs.ToList().Select(x => @"{ ""command"":""subscribe"", ""channel"":""" + x.Value.TradePair.ToCurrency.Code + "_" + x.Value.TradePair.FromCurrency.Code + @"""}").ToList();

            // Form the initiation string
            // messages.Insert(0, @"{ ""command"":""subscribe"", ""channel"":1002}"); // This to get the ticker.

            string[] messages = new[] { @"{ ""command"":""subscribe"", ""channel"":""ticker""}" }; // This to get the ticker.


#warning    This is still all hardcoded here!
            var uri = "wss://api.poloniex.com";

            // And initialize the websocket, pasing ParseResult as the callback to parse data from Polonies
            this.websocket = await Helpers.WebSocketManager.Initialize(uri, messages.ToArray(), ParseResult);

        }

        public override async Task ShutDownAsync()
        {
            await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }



        // Callback when poloniex send information.
        private async Task ParseResult(string result)
        {
            // We just want matches. Discard everything else.
            if (result.Contains("match"))
            {
                var jsonObj = JsonConvert.DeserializeObject<PoloniexTickerJson>(result);



                PriceChangeEvent priceChange = new PriceChangeEvent()
                {
                    //Price =  jsonObj.price,
                    //UtcTime = DateTime.Parse(jsonObj.time).ToUniversalTime(),
                    //ExchangeTradePair = this.GetExchangeTradePair(jsonObj.product_id.Split('-')[0], jsonObj.product_id.Split('-')[1])

                };

                // Notify the observers!
                await this.NotifyObserverOfPriceChange(priceChange);
            }

        }
    }
}
