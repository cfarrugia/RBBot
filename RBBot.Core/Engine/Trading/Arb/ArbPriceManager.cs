using RBBot.Core.Engine.Trading;
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
    /// <summary>
    /// The arb manager works by maintaining a matrix per trade-pair. Think of a 3dimensional matrix with first dimension 
    /// </summary>
    public class ArbPriceManager : IMarketPriceProcessor 
    {
        #region Singleton initialization

        private static volatile ArbPriceManager instance;
        private static object syncRoot = new Object();

        private ArbPriceManager() { }
        
        /// <summary>
        /// We just want one instance of the market price observer.
        /// </summary>
        public static ArbPriceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ArbPriceManager();
                    }
                }

                return instance;
            }
        }

        #endregion

#warning Should be a setting really.
        // After these many seconds, if the price wasn't updated, it's invalidated.
        private const int priceInvalidationDelaySeconds = 120;

        public async Task<IEnumerable<Opportunity>> OnMarketPriceChangeAsync(ExchangeTradePair changedPair)
        {
            // Ignore non crypto trade pairs!
            if (!changedPair.TradePair.FromCurrency.IsCrypto || !changedPair.TradePair.ToCurrency.IsCrypto) return new Opportunity[] { };

            // Get all the other exchange pairs. Shoot down old prices. 
            var otherExchangePairs = TradePriceIndex.GetExchangeTradePairs(changedPair.TradePair).Where(x => x != changedPair && x.LatestUpdate.AddSeconds(priceInvalidationDelaySeconds) >= DateTime.UtcNow).ToList();

            // Take tuples with lower price as the first item. 
            var opportunities =
                otherExchangePairs.Select(x => new Tuple<ExchangeTradePair, ExchangeTradePair, decimal>(x, changedPair, CalculateOpportunityMarginPercent(x, changedPair)))
                .Union(
                otherExchangePairs.Select(x => new Tuple<ExchangeTradePair, ExchangeTradePair, decimal>(changedPair, x, CalculateOpportunityMarginPercent(x, changedPair))))
                .Select(x => new ArbOpportunity(x.Item1, x.Item2))
                .ToList();
            //            
            return opportunities;
        }


        /// <summary>
        /// Gets the key describing an arb opportunity.
        /// </summary>
        internal static string GetOpportunityKey(ExchangeTradePair lowerPricePair, ExchangeTradePair higherPricePair)
        {
            return lowerPricePair.TradePair.ToString() + " - " + lowerPricePair.Exchange.ToString() + " -> " + higherPricePair.Exchange.ToString();
        }

        internal static decimal CalculateOpportunityMarginPercent(ExchangeTradePair lowerPricePair, ExchangeTradePair higherPricePair)
        {
            // The opportunity is calculated by comparing the higher price with the lower price.
            decimal marginPercent = ((higherPricePair.LatestPrice / lowerPricePair.LatestPrice) - 1m) * 100m;

            // Then we remove the fees on both sides.
            marginPercent -= (higherPricePair.FeePercent + lowerPricePair.FeePercent);

#warning Still need to calculated the amount for transfering and balancing accounts!!


            // Else just filter to zero
            return marginPercent;
        }
    }
}
