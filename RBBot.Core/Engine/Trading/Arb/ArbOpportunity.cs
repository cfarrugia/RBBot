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


        public override Task<TradeOpportunityRequirement[]> GetAndCheckRequirements()
        {
            // The requirements for an arb opportunity, aside from the actual arb itself, is to have enough funds to execute it.
            


#warning Implement
            throw new NotImplementedException();
        }


        public override ITradeAction GetTradeAction(decimal amount)
        {
            // We will run through an example. Let's say the arb is between ETH -> BTC. 
            // On Ex1, ETH/BTC = 0.015
            // On Ex2, ETH/BTC = 0.017
            // We want to Buy ETH on ex1 and sell ETH on ex2
            // This means we need to have BTC on ex1
            // This means we need to have ETH on ex2
            // Let's assume we do, and the arb can be executed. 
            // And let's assume we do a 1ETH trade.
            // On EX1 we take 0.015BTC and buy 1ETH (the lower price pair!)
            // On EX2 we take 1ETH and buy 0.017BTC (the higher price pair!)
            // At the end i will find myself with an additional 0.017 - 0.015 = 0.002BTC.
            return new NullAction()
            {
                ChildrenActions = new ITradeAction[]
                {
                    new ExchangeOrderAction(this.LowerPricePair.ExchangeTradePair, ExchangeOrderType.Buy, amount),
                    new ExchangeOrderAction(this.HigherPricePair.ExchangeTradePair, ExchangeOrderType.Sell, amount)
                }
            };
        }

        public override decimal GetMaximumAmountThatCanBeTransacted()
        {
            throw new NotImplementedException();
        }

        internal ArbOpportunity(TradePairPrice lowerPricePair, TradePairPrice higherPricePair, Currency currency)
        {
            this.LowerPricePair = lowerPricePair;
            this.HigherPricePair = higherPricePair;
            this.OpportunityBaseCurrency = currency;

        }

    }
}
