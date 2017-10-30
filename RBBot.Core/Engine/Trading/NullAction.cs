using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RBBot.Core.Models;

namespace RBBot.Core.Engine.Trading
{
    public class NullAction : ITradeAction
    {
        public Currency BaseCurrency { get { return null; } }

        public ITradeAction[] ChildrenActions { get; set; }

        public decimal EstimatedCost { get { return 0m; } }

        public TimeSpan EstimatedTimeToExecute { get { return new TimeSpan(0); } }

        public bool ExecuteBeforeChildren { get; set; }
        public bool ExecuteChildrenInParallel { get; set; }

        public decimal MaxExposureCost { get { return 0m; } }

        
        public Task<TradeOpportunityTransaction> ExecuteAction(bool simulate)
        {
            // Do nothing.
            return Task.FromResult<TradeOpportunityTransaction>(null);
        }
    }
}
