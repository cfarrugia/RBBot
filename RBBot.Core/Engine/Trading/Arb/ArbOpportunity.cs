using RBBot.Core.Database;
using RBBot.Core.Engine.Trading.Actions;
using RBBot.Core.Helpers;
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
        public ExchangeTradePair LowerPricePair { get; private set; }

        /// <summary>
        /// This is the higher trade price
        /// </summary>
        public ExchangeTradePair HigherPricePair { get; private set; }

        private TradeAccount LowerAccount;

        private TradeAccount HigherAccount;

        public override string TypeCode { get { return "CRYPT-ARB"; } }

        public override string UniqueIdentifier 
        {
            get
            {
                // The unique identifier is the opportunity type, and the lower to higher tradepairs.
                return $"{this.TypeCode} | {this.LowerPricePair} -> {this.HigherPricePair}";
            }
        }


        #region Object overrides
        public override bool Equals(object obj)
        {

            if (!(obj is ArbOpportunity)) return false;

            var tp = (ArbOpportunity)obj;

            return tp.LowerPricePair == this.LowerPricePair && tp.HigherPricePair == this.HigherPricePair;
        }

        /// <summary>
        /// Please make sure you don't have more than 10000 trade pairs. I hope not :) 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (this.HigherPricePair.GetHashCode() * 10000) + this.LowerPricePair.GetHashCode();
        }


        public override string ToString()
        {
            return this.TypeCode + " | " + ArbPriceManager.GetOpportunityKey(this.LowerPricePair, this.HigherPricePair);
        }

        #endregion


        public override TradeOpportunityRequirement[] GetAndCheckRequirements()
        {
            List<TradeOpportunityRequirement> requirements = new List<TradeOpportunityRequirement>();

            requirements.Add(GetExchangeTradeRequirement(this.LowerPricePair.Exchange));
            requirements.AddRange(GetAccountRequirements("Lower-Price", this.LowerPricePair, this.LowerPricePair.TradePair.ToCurrency, out this.LowerAccount)); // We want to SELL ToCurrency from lower exchange

            requirements.Add(GetExchangeTradeRequirement(this.HigherPricePair.Exchange));
            requirements.AddRange(GetAccountRequirements("Higher-Price", this.HigherPricePair, this.HigherPricePair.TradePair.FromCurrency, out this.HigherAccount)); // We want to SELL FromCurrency from higher exchange

            // Another requirement is that any one of the two currencies need to be the preferred cryptocurrency
            bool requirement = this.LowerPricePair.TradePair.FromCurrency == SystemSetting.PreferredCyptoCurrency || this.LowerPricePair.TradePair.ToCurrency == SystemSetting.PreferredCyptoCurrency;
            requirements.Add(new TradeOpportunityRequirement()
            {
                RequirementMet = requirement,
                ItemIdentifier = $"Preferred CryptoCurrency Missing - {this.LowerPricePair.TradePair}", 
                Message = requirement ? "" : $"The tradepair {this.LowerPricePair.TradePair} doesn't include the preferred crypto-currency {SystemSetting.PreferredCyptoCurrency}. This is currently unsupported.",
                Timestamp = DateTime.UtcNow,
                TradeOpportunityRequirementTypeId = TradeOpportunityRequirementType.RequirementTypes.Where(x => x.Code == "SUPPORT-CURR").Single().Id,

            });

            return requirements.ToArray();
        }

        /// <summary>
        /// Our aim is to improve the number of "preferred crypto-currency coins" (i.e. BTC at the time of writing). To do so, and be less exposed we need to sell as much as we can 
        /// of the non-preferred currency and gain as much as we can of the preferred one.
        /// 
        /// </summary>
        /// <returns></returns>
        public override decimal GetMaximumAmountThatCanBeTransacted()
        {
            //
            int preferredIndex = this.LowerPricePair.TradePair.FromCurrency == SystemSetting.PreferredCyptoCurrency ? 0 : 1;

            // Make sure both accounts exist. If not return 0m already. 
            if (this.LowerAccount == null || this.HigherAccount == null) return 0m;

            // 
            var lowerExchangeAvailability = this.LowerAccount.Currency == SystemSetting.PreferredCyptoCurrency ? this.LowerAccount.Balance : (preferredIndex == 0 ? (this.LowerAccount.Balance / this.LowerPricePair.LatestPrice) : (this.LowerAccount.Balance * this.LowerPricePair.LatestPrice));
            var higherExchangeAvailability = this.HigherAccount.Currency == SystemSetting.PreferredCyptoCurrency ? this.HigherAccount.Balance : (preferredIndex == 0 ? (this.HigherAccount.Balance / this.HigherPricePair.LatestPrice) : (this.HigherAccount.Balance * this.HigherPricePair.LatestPrice));

            // The minimum of both is the maximum that can be transacted
            return Math.Min(lowerExchangeAvailability, higherExchangeAvailability);
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
                    new ExchangeOrderAction(this.LowerPricePair, ExchangeOrderType.Buy, amount),
                    new ExchangeOrderAction(this.HigherPricePair, ExchangeOrderType.Sell, amount)
                },
                ExecuteChildrenInParallel = true
            };
        }

       

        public override decimal GetValue()
        {
            return ArbPriceManager.CalculateOpportunityMargin(this.LowerPricePair, this.HigherPricePair);
        }

        internal ArbOpportunity(ExchangeTradePair lowerPricePair, ExchangeTradePair higherPricePair)
        {
            this.LowerPricePair = lowerPricePair;
            this.HigherPricePair = higherPricePair;

            // The opportunity's base currency is always the preferred cryptocurrency. 
            this.OpportunityBaseCurrency = SystemSetting.PreferredCyptoCurrency;

        }

    }
}
