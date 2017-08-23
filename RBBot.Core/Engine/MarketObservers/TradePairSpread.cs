using RBBot.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.MarketObservers
{
    public class TradePairPrice : IComparable<TradePairPrice>
    {
        public decimal Price { get; set; }
        public DateTime UtcLastUpdateTime { get; set; }
        public ExchangeTradePair ExchangeTradePair { get; set; }

        public int AgeMilliseconds { get { return (DateTime.UtcNow - this.UtcLastUpdateTime).Milliseconds; } }

        public int CompareTo(TradePairPrice other)
        {
            if (this.Price > other.Price)
                return 1;
            if (this.Price < other.Price)
                return -1;
            else
                return 0;
        }
    }
    public class TradePairSpread
    {
        private ConcurrentDictionary<Exchange, TradePairPrice> priceDic = null;
        public TradePairSpread(TradePair tradePair)
        {
            this.TradePair = tradePair;
            priceDic = new ConcurrentDictionary<Exchange, TradePairPrice>();
            this.MaximumValuedPair = this.MinimumValuedPair = null;
        }

        public async Task UpdatePrice(ExchangeTradePair exchangeTradePair, decimal price, DateTime utcPriceUpdateTime)
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


            //
            this.MaximumValuedPair = this.priceDic.Max(x => x.Value);
            this.MinimumValuedPair = this.priceDic.Min(x => x.Value);

            // 
            decimal marginPercent = ((this.MaximumValuedPair.Price / this.MinimumValuedPair.Price) - 1m) * 100m;
            if (marginPercent > 1.5m && this.TradePair.FromCurrency.IsCrypto && this.TradePair.ToCurrency.IsCrypto)
            {
                int milliSecondSinceUpdate = Math.Max(MaximumValuedPair.AgeMilliseconds, MinimumValuedPair.AgeMilliseconds);
                Console.WriteLine($"Trade Opportunity of {marginPercent:0.00}% for {this.TradePair} with Price on {this.MinimumValuedPair.ExchangeTradePair.Exchange} of {this.MinimumValuedPair.Price}/ Age:{this.MinimumValuedPair.AgeMilliseconds}ms and {this.MaximumValuedPair.ExchangeTradePair.Exchange} of {this.MaximumValuedPair.Price} / Age:{this.MaximumValuedPair.AgeMilliseconds}ms ");
            }
                
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
