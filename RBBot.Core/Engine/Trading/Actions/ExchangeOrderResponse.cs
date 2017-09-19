using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading.Actions
{
    public class ExchangeOrderResponse
    {
        public bool Success { get; set; }
        public string ExternalTransactionId { get; set; }

        public TransactionFee Fee { get; set; }


    }
}
