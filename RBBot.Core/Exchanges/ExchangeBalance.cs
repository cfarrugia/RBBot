using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges
{
    public class ExchangeBalance
    {
        public Exchange Exchange { get; private set; }
        public decimal Balance { get; private set;  }
        public decimal Timestamp { get; private set; }
        public Currency Currency { get; private set; }

        public ExchangeBalance(Exchange exchange, decimal balance, decimal utcTimestamp, Currency currency)
        {
            this.Exchange = exchange;
            this.Balance = balance;
            this.Timestamp = utcTimestamp;
            this.Currency = currency;
        }
    }
}
