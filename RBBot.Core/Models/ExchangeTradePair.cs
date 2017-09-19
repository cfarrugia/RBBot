namespace RBBot.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ExchangeTradePair")]
    public partial class ExchangeTradePair
    {
        #region Object overrides
        public override bool Equals(object obj)
        {

            if (!(obj is ExchangeTradePair)) return false;

            var tp = (ExchangeTradePair)obj;

            return tp.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return this.Id;
        }

        public override string ToString()
        {
            return this.Exchange.ToString() + " - " + this.TradePair.ToString() + $" ({this.ExchangeTradePairState.Code})";
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ExchangeTradePair()
        {
            MarketPrices = new HashSet<MarketPrice>();
        }

        public int Id { get; set; }

        public int TradePairId { get; set; }

        public int ExchangeId { get; set; }

        public decimal FeePercent { get; set; }

        public int StateId { get; set; }

        public virtual Exchange Exchange { get; set; }

        public virtual ExchangeTradePairState ExchangeTradePairState { get; set; }

        public virtual TradePair TradePair { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MarketPrice> MarketPrices { get; set; }

        /// <summary>
        /// This is not mapped to DB. We keep this internally as we need to know what the last price was.
        /// </summary>
        public decimal LatestPrice { get; set; }
    }
}
