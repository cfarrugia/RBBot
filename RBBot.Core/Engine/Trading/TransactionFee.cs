using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading
{
    public class TransactionFee
    {
        public Currency Currency { get; set; }
        public decimal Amount { get; set; }
    }
}
