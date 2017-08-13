using System;
using System.Collections.Generic;

namespace RBBot.Core.Models
{
    public partial class TradePair
    {
        public TradePair()
        {
            ExchangeTradePair = new HashSet<ExchangeTradePair>();
        }

        public int Id { get; set; }
        public int FromCurrencyId { get; set; }
        public int ToCurrencyId { get; set; }

        public virtual ICollection<ExchangeTradePair> ExchangeTradePair { get; set; }
        public virtual Currency FromCurrency { get; set; }
        public virtual Currency ToCurrency { get; set; }
    }
}
