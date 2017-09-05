using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading.Actions
{
    /// <summary>
    /// This is the class responsable for the manifestation of an order. 
    /// </summary>
    public class ExchangeOrderAction : ITradeAction
    {
        /// <summary>
        /// The trade pair for which the order will be executed.
        /// </summary>
        public ExchangeTradePair TradePair { get; set; }

        public decimal Cost 
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Currency BaseCurrency
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ExchangeOrderAction(ExchangeTradePair tradePair, decimal amountInFromCurrency)
        {
        }

        public void ExecuteAction(bool simulate)
        {
            throw new NotImplementedException();
        }
    }
}
