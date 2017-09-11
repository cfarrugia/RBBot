using RBBot.Core.Database;
using RBBot.Core.Engine.Trading.Actions;
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
    /// This object tracks an arbitrage opportunity. The object will keep on having its state changed when the price changes. 
    /// In essense the opportunity consists of two exchanges/trade-pairs and the amount to be made as a percentage of the trade.
    /// Please note we don't want to keep on constructing a different opportunity each time, and therefore Opportunities are created or fetched by a factory.
    /// </summary>
    public class ArbOpportunity : Opportunity
    {
        private ArbOpportunity() { }

        /// <summary>
        /// This is the lower trade price
        /// </summary>
        public TradePairPrice LowerPricePair { get; private set; }

        /// <summary>
        /// This is the higher trade price
        /// </summary>
        public TradePairPrice HigherPricePair { get; private set; }

        public override string TypeCode { get { return "CRYPT-ARB"; } }


        #region Object overrides
        public override bool Equals(object obj)
        {

            if (!(obj is ArbOpportunity)) return false;

            var tp = (ArbOpportunity)obj;

            return tp.LowerPricePair.ExchangeTradePair == this.LowerPricePair.ExchangeTradePair && tp.HigherPricePair.ExchangeTradePair == this.HigherPricePair.ExchangeTradePair;
        }

        /// <summary>
        /// Please make sure you don't have more than 10000 trade pairs. I hope not :) 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (this.HigherPricePair.ExchangeTradePair.GetHashCode() * 10000) + this.LowerPricePair.ExchangeTradePair.GetHashCode();
        }


        public override string ToString()
        {
            return ArbPriceManager.GetOpportunityKey(this.LowerPricePair, this.HigherPricePair);
        }

        #endregion

        internal ArbOpportunity(TradePairPrice lowerPricePair, TradePairPrice higherPricePair, Currency currency)
        {
            this.LowerPricePair = lowerPricePair;
            this.HigherPricePair = higherPricePair;
            this.OpportunityBaseCurrency = currency;

            // The actions to be taken 

            this.Actions = new ITradeAction[][]
            {
                new ITradeAction [] { },
                new ITradeAction []
                {
                    //new ExchangeOrderAction()

                }

            };

        }

    }
}
