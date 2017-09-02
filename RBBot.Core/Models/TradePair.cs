namespace RBBot.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TradePair")]
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TradePair()
        {
            ExchangeTradePairs = new HashSet<ExchangeTradePair>();
        }

        public int Id { get; set; }

        public int FromCurrencyId { get; set; }

        public int ToCurrencyId { get; set; }

        public virtual Currency FromCurrency { get; set; }

        public virtual Currency ToCurrency { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExchangeTradePair> ExchangeTradePairs { get; set; }
    }
}
