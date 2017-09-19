using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RBBot.Core.Models;

namespace RBBot.Core.Engine.Trading.Triangulation
{
    public class TriangularationOpportunity : Opportunity
    {
        public override string TypeCode { get { return "EXC-TRIAN"; } }
        private TriangularationOpportunity() { }

#warning implement
        public override Task<TradeOpportunityRequirement[]> GetAndCheckRequirements()
        {
            throw new NotImplementedException();
        }

        public override decimal GetMaximumAmountThatCanBeTransacted()
        {
            throw new NotImplementedException();
        }

        public override ITradeAction GetTradeAction(decimal amount)
        {
            throw new NotImplementedException();
        }
    }
}
