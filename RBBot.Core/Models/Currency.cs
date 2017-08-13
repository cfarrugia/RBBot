using System;
using System.Collections.Generic;

namespace RBBot.Core.Models
{
    public partial class Currency
    {
        public Currency()
        {
            TradePairFromCurrency = new HashSet<TradePair>();
            TradePairToCurrency = new HashSet<TradePair>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsCrypto { get; set; }

        public virtual ICollection<TradePair> TradePairFromCurrency { get; set; }
        public virtual ICollection<TradePair> TradePairToCurrency { get; set; }
    }
}
