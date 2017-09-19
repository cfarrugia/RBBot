using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading
{
    public interface ITradeAction
    {
        /// <summary>
        /// Pointer to the children action, if any.
        /// </summary>
        ITradeAction[] ChildrenActions { get; set; }

        /// <summary>
        /// Flags whether it is safe to run children in parallel.
        /// </summary>
        bool ExecuteChildrenInParallel {get; set; }

        /// <summary>
        /// Flags whether this action should be run before or after the children have executed.
        /// </summary>
        bool ExecuteBeforeChildren { get; set; }

        /// <summary>
        /// The actual cost of doing this action (for a currency transfer this is a fixed rate. For an exchange this is a percentage of the base currency amount)
        /// </summary>
        decimal EstimatedCost { get; }

        /// <summary>
        /// Using the volatility index of the trade pair, we can calculate how much we'd be exposed with this trading pair 
        /// </summary>
        decimal MaxExposureCost { get; }

        /// <summary>
        /// This is the currency in which the costs are calculated
        /// </summary>
        Currency BaseCurrency { get; }

        /// <summary>
        /// The estimated time it takes for the action to be done.
        /// </summary>
        TimeSpan EstimatedTimeToExecute { get; }

        /// <summary>
        /// Returns the model representing the transaction linked to the action taken.
        /// </summary>
        /// <param name="simulate">flag simulation or real actions.</param>
        /// <returns></returns>
        Task<TradeOpportunityTransaction> ExecuteAction(bool simulate);
    }
}
