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
        public decimal Balance { get; set; }
        public DateTime Timestamp { get; private set; }
        public string CurrencyCode { get; private set; }

        public string ExchangeIdentifier { get; set; }

        public string Address { get; set; }

        public ExchangeBalance(Exchange exchange, decimal balance, DateTime utcTimestamp, string currencyCode, string Address = null, string ExchangeIdentifier = null)
        {
            this.Exchange = exchange;
            this.Balance = balance;
            this.Timestamp = utcTimestamp;
            this.CurrencyCode = currencyCode;
            this.Address = Address;
            this.ExchangeIdentifier = ExchangeIdentifier;
        }
    }
}
