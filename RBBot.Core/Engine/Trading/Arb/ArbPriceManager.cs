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

#warning make this a setting in db.
        /// <summary>
        /// This is tminimum threshold percent AFTER FEES are considered to be considered as an opportunity. Anything less is not.
        /// </summary>
        internal const decimal MinimumOpportunityTreshold = 1.00m;

        private ConcurrentDictionary<TradePair, ArbTradePairSpread> spreadPerTradePair = new ConcurrentDictionary<TradePair, ArbTradePairSpread>();


        private ConcurrentDictionary<string, ArbOpportunity> opportunities = new ConcurrentDictionary<string, ArbOpportunity>();


        public async Task<IEnumerable<Opportunity>> OnMarketPriceChangeAsync(PriceChangeEvent change)
        {
            // Ignore non crypto trade pairs!
            if (!change.ExchangeTradePair.TradePair.FromCurrency.IsCrypto || !change.ExchangeTradePair.TradePair.ToCurrency.IsCrypto) return new Opportunity[] { };

            // Get trade pair spread. 
            ArbTradePairSpread tpSpread = this.spreadPerTradePair.GetOrAdd(change.ExchangeTradePair.TradePair, new ArbTradePairSpread(change.ExchangeTradePair.TradePair));

            // Update price.
            tpSpread.UpdatePrice(change.ExchangeTradePair, change.Price, change.UtcTime);


            // Calculate the margin 
            var margin = CalculateOpportunityMargin(tpSpread.MinimumValuedPair, tpSpread.MaximumValuedPair);

            // If margin is zero, then return. 
            if (margin == 0m) return new Opportunity[] { };

            // Get or add the opportunity object.
#warning Not sure how to define the currency. This is somehow the base currency.
            var opportunity = opportunities.GetOrAdd(GetOpportunityKey(tpSpread.MinimumValuedPair, tpSpread.MaximumValuedPair), new ArbOpportunity(tpSpread.MinimumValuedPair, tpSpread.MaximumValuedPair, tpSpread.MinimumValuedPair.ExchangeTradePair.TradePair.ToCurrency));

            await opportunity.UpdateOpportunity(margin);

            //
            Console.WriteLine($"Trade Opportunity of {margin:0.00}% for {tpSpread.MinimumValuedPair.ExchangeTradePair.TradePair} with Price on {tpSpread.MinimumValuedPair.ExchangeTradePair.Exchange} of {tpSpread.MinimumValuedPair.Price}/ Age:{tpSpread.MinimumValuedPair.AgeMilliseconds}ms and {tpSpread.MaximumValuedPair.ExchangeTradePair.Exchange} of {tpSpread.MaximumValuedPair.Price} / Age:{tpSpread.MaximumValuedPair.AgeMilliseconds}ms ");

#warning Rewrite this method to get a list of opportunities!
            // 
            return new Opportunity[] { opportunity };
        }


        /// <summary>
        /// Gets the key describing an arb opportunity.
        /// </summary>
        internal static string GetOpportunityKey(TradePairPrice lowerPricePair, TradePairPrice higherPricePair)
        {
            return lowerPricePair.ExchangeTradePair.TradePair.ToString() + " - " + lowerPricePair.ExchangeTradePair.Exchange.ToString() + " -> " + higherPricePair.ExchangeTradePair.Exchange.ToString();
        }

        private static decimal CalculateOpportunityMargin(TradePairPrice lowerPricePair, TradePairPrice higherPricePair)
        {
            // The opportunity is calculated by comparing the higher price with the lower price.
            decimal marginPercent = ((higherPricePair.Price / lowerPricePair.Price) - 1m) * 100m;

            // Then we remove the fees on both sides.
            marginPercent -= (higherPricePair.ExchangeTradePair.FeePercent + lowerPricePair.ExchangeTradePair.FeePercent);

#warning Still need to calculated the amount for transfering and balancing accounts!!


            // If the margin percent is higher than the minimum opportunity threshold, then report it.
            if (marginPercent >= MinimumOpportunityTreshold) return marginPercent;

            // Else just filter to zero
            return 0m;
        }
    }
}
