using RBBot.Core.Engine;
using RBBot.Core.Engine.MarketObservers;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges
{
    public abstract class ExchangeIntegration : IExchangeIntegration
    {
        /// <summary>
        /// The constructor of the integration
        /// </summary>
        /// <param name="priceObservers">The list of price observers</param>
        /// <param name="exchange">The full exchange db object (with all children items included).</param>
        public ExchangeIntegration(IMarketPriceObserver[] priceObservers, Exchange[] exchanges)
        {
            //
            this.PriceObservers = priceObservers;

            //
            this.PriceObservers = priceObservers;

            tradingPairs = new Dictionary<string, Models.ExchangeTradePair>();

            foreach (var exchange in exchanges)
            {
                foreach (var pair in exchange.ExchangeTradePair.Where(x => x.Status.Code != "OFF").ToList())
                {
                    if (pair.Status.Code == "OFF") continue; // Ignore offline pairs.
                    tradingPairs.Add(GetPairKey(exchange.Name, pair.TradePair.FromCurrency.Code, pair.TradePair.ToCurrency.Code), pair);
                }
            }
        }

        
        protected Dictionary<string, ExchangeTradePair> tradingPairs = null;

        public abstract string Name { get; }

        public IEnumerable<IMarketPriceObserver> PriceObservers { get; set; }

        public abstract Task InitializeAsync();

        public abstract Task ShutDownAsync();


        protected async Task NotifyObserverOfPriceChange(PriceChangeEvent priceEvent)
        {
            // Loop through the observers and signal change.
            foreach (var observer in this.PriceObservers)
            {
                await observer.OnMarketPriceChangeAsync(priceEvent);
            }

        }

        protected string GetPairKey(string exchangeName, string fromCurrencyCode, string ToCurrencyCode)
        {
            return exchangeName + "-" + fromCurrencyCode + "-" + ToCurrencyCode;
        }

        /// <summary>
        /// Helper method to get the trading pair db object from currency codes.
        /// </summary>
        protected ExchangeTradePair GetExchangeTradePair(string exchange, string fromCurrencyCode, string toCurrencyCode)
        {
            return this.tradingPairs[GetPairKey(exchange, fromCurrencyCode, toCurrencyCode)];
        }
    }
}
