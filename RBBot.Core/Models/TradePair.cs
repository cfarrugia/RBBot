using System;
using System.Collections.Generic;

namespace RBBot.Core.Models
{
    public partial class TradePair
    {
        #region Object overrides
        public override bool Equals(object obj)
        {

            if (!(obj is TradePair)) return false;

            var tp = (TradePair)obj;

            return tp.FromCurrencyId == this.FromCurrencyId && tp.ToCurrencyId == this.ToCurrencyId;
        }

        public override int GetHashCode()
        {
            return this.FromCurrencyId * 1000 + this.ToCurrencyId;
        }

        public override string ToString()
        {
            return this.FromCurrency.Code + "-" + this.ToCurrency.Code;
        }

        #endregion

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
