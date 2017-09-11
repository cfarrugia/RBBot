using RBBot.Core.Engine.Trading.Arb;
using RBBot.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading.Arb
{
    
    public class ArbTradePairSpread
    {
        private ConcurrentDictionary<Exchange, TradePairPrice> priceDic = null;

        public ArbTradePairSpread(TradePair tradePair)
        {
            this.TradePair = tradePair;
            priceDic = new ConcurrentDictionary<Exchange, TradePairPrice>();
            this.MaximumValuedPair = this.MinimumValuedPair = null;
        }

        public void UpdatePrice(ExchangeTradePair exchangeTradePair, decimal price, DateTime utcPriceUpdateTime)
        {

            // We want to do the following:
            // If the price is older than priceInvalidationDelaySeconds, then just remove it.
            // If you find the exchange, delete it. 
            // Calculate the min and max again.

            var exchangesToRemove = priceDic.Values.Where(x => x.UtcLastUpdateTime.AddSeconds(priceInvalidationDelaySeconds) > DateTime.Now).Select(x => x.ExchangeTradePair.Exchange);
            TradePairPrice priceRemoved = null;
            foreach (var exchangeToRemove in exchangesToRemove) priceDic.TryRemove(exchangeToRemove, out priceRemoved);

            // Get latest price or add it.
            var latestPrice = priceDic.AddOrUpdate(
                exchangeTradePair.Exchange,
                new TradePairPrice() { ExchangeTradePair = exchangeTradePair, Price = price, UtcLastUpdateTime = utcPriceUpdateTime },
                (e, oldVal) => {
                    oldVal.ExchangeTradePair = exchangeTradePair;
                    oldVal.Price = price;
                    oldVal.UtcLastUpdateTime = utcPriceUpdateTime;
                    return oldVal; }
                );


            // Now that the maximum and minimum have been calculated, we want to pass the ball to the Arb
            this.MaximumValuedPair = this.priceDic.Max(x => x.Value);
            this.MinimumValuedPair = this.priceDic.Min(x => x.Value);
        }


        #warning Should be a setting really.
        // After these many seconds, if the price wasn't updated, it's invalidated.
        private const int priceInvalidationDelaySeconds = 120;




        /// <summary>
        /// Think of the trade pair as the key of the object.
        /// </summary>
        public TradePair TradePair { get; private set; }

        public TradePairPrice MinimumValuedPair { get; set; }
        public TradePairPrice MaximumValuedPair { get; set; }

       



    }
}
