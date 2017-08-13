using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RBBot.Core.Engine;
using System.Net.WebSockets;
using System.Threading;
using Newtonsoft.Json;
using RBBot.Core.Models;

namespace RBBot.Core.Exchanges.GDAX
{
    public class GDAXIntegration : ExchangeIntegration
    {
        
        public override string Name { get { return "GDAX"; } }

        private ClientWebSocket websocket = null;

        public GDAXIntegration(IEnumerable<IPriceObserver> priceObservers, Exchange exchange) : base(priceObservers, exchange)
        {
            
        }

        public override async Task InitializeAsync()
        {
            // Get all the products from the trading pairs.
            string products = string.Join(",", this.tradingPairs.ToList().Select(x => "\"" + x.Value.TradePair.FromCurrency.Code + "-" + x.Value.TradePair.ToCurrency.Code + "\""));

            // Form the initiation string
            var text = @"{ ""type"": ""subscribe"", ""product_ids"": [" + products + "] }";

#warning    This is still all hardcoded here!
            var uri = "wss://ws-feed.gdax.com";

            // And initialize the websocket, pasing ParseResult as the callback to parse data from GDAX
            this.websocket = await Helpers.WebSocketManager.Initialize(uri, text, ParseResult);

        }

        public override async Task ShutDownAsync()
        {
            await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }



        // Callback when gdax send information.
        private async Task ParseResult(string result)
        {
            // We just want matches. Discard everything else.
            if (result.Contains("match"))
            {
                var jsonObj = JsonConvert.DeserializeObject<GDAXTradeMatchJson>(result);



                PriceChangeEvent priceChange = new PriceChangeEvent()
                {
                    Price = jsonObj.price,
                    UtcTime = DateTime.Parse(jsonObj.time).ToUniversalTime(),
                    ExchangeTradePair = this.GetExchangeTradePair(jsonObj.product_id.Split('-')[0], jsonObj.product_id.Split('-')[1])

                };

                // Notify the observers!
                await this.NotifyObserverOfPriceChange(priceChange);
            }

        }

    }
}
