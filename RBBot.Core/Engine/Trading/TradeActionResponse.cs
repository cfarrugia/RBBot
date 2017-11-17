using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading
{
    public class TradeActionResponse
    {
        public TradeOpportunityTransaction[] Transactions { get; set; }

        public bool ExecutionSuccessful { get; set; } = true;
    }
}
