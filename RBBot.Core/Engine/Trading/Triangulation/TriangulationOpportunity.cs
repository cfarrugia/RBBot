using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RBBot.Core.Models;
using RBBot.Core.Engine.Trading.Actions;

namespace RBBot.Core.Engine.Trading.Triangulation
{
    public class TriangulationOpportunity : Opportunity
    {
        public override string TypeCode { get { return "EXC-TRIAN"; } }


        private ExchangeTriangulation triangulation = null;
        private ExchangeTriangulationEdge initialEdge = null;

        private TriangulationOpportunity() { }

        internal TriangulationOpportunity(ExchangeTriangulation triangulation)
        {
            this.triangulation = triangulation;
            this.initialEdge = this.triangulation.Edges[0];
            this.OpportunityBaseCurrency = initialEdge.IsReversed ? initialEdge.CurrentPrice.TradePair.ToCurrency : initialEdge.CurrentPrice.TradePair.FromCurrency;
        }

        /// <summary>
        /// This is the account from where the opportunity will start. We keep this for quick reference. 
        /// </summary>
        private TradeAccount InitialAccount = null;
        
        public override string UniqueIdentifier
        {
            get
            {
                // The unique identifier is the opportunity type, and the lower to higher tradepairs.
                return $"{this.TypeCode} | {this.triangulation.GenericDescription}";
            }
        }


        public override TradeOpportunityRequirement[] GetAndCheckRequirements()
        {
            List<TradeOpportunityRequirement> requirements = new List<TradeOpportunityRequirement>();

            var exchange = this.triangulation.Edges[0].CurrentPrice.Exchange;

            // The first requirement is that the exchange is implemented.
            requirements.Add(GetExchangeTradeRequirement(exchange));

            // The second requirement is to have the initial account from where to start the trade exists.
            requirements.AddRange(GetAccountRequirements("Initiating", initialEdge.CurrentPrice, this.OpportunityBaseCurrency, out this.InitialAccount));

            return requirements.ToArray();
        }

        /// <summary>
        /// Gets the maximum amount that can be transacted. It is assumed here that the opportunity requirements have been checked beforehand!
        /// </summary>
        /// <returns></returns>
        public override decimal GetMaximumAmountThatCanBeTransacted()
        {
            if ((this.InitialAccount == null) || (this.InitialAccount.Balance <= 0m))
                    return 0m;

            return this.InitialAccount.Balance;
        }

        public override ITradeAction GetTradeAction(decimal amount)
        {
            throw new NotImplementedException("The following line doesn't function properly. watch out!");


            return new NullAction()
            {
                // The children actions will be a buy sell if reversed and buy if not.
                ChildrenActions = this.triangulation.Edges.Select(x => new ExchangeOrderAction(x.CurrentPrice, x.IsReversed ? ExchangeOrderType.Sell : ExchangeOrderType.Buy, amount, x.CurrentPrice.TradePair.FromCurrency)).ToArray(),
                ExecuteChildrenInParallel = false // We need to sequence this in order!
            };
        }

        public override decimal GetMarginValuePercent()
        {
            return this.triangulation.GetValue();
        }
    }
}
