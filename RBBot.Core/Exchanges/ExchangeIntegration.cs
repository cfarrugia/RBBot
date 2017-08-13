using RBBot.Core.Engine;
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
        public ExchangeIntegration(IEnumerable<IPriceObserver> priceObservers, Exchange exchange)
        {
            //
            this.PriceObservers = priceObservers;


            tradingPairs = exchange.ExchangeTradePair.ToDictionary(x => x.TradePair.FromCurrency.Code + "-" + x.TradePair.ToCurrency.Code, y => y);
        }

        private ExchangeIntegration() { }

        protected Dictionary<string, ExchangeTradePair> tradingPairs = null;

        public abstract string Name { get; }

        public IEnumerable<IPriceObserver> PriceObservers { get; set; }

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

        /// <summary>
        /// Helper method to get the trading pair db object from currency codes.
        /// </summary>
        protected ExchangeTradePair GetExchangeTradePair(string fromCurrencyCode, string toCurrencyCode)
        {
            return this.tradingPairs[fromCurrencyCode + "-" + toCurrencyCode];
        }
    }
}
