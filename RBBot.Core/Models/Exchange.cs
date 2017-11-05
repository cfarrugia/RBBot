namespace RBBot.Core.Models
{
    using Exchanges;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Exchange")]
    public partial class Exchange
    {
        #region Object overrides
        public override bool Equals(object obj)
        {

            if (!(obj is Exchange)) return false;

            var tp = (Exchange)obj;

            return tp.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return this.Id;
        }

        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Exchange()
        {
            Settings = new HashSet<Setting>();
            ExchangeTradePairs = new HashSet<ExchangeTradePair>();
            TradeAccounts = new HashSet<TradeAccount>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public int StateId { get; set; }

        public virtual ExchangeState ExchangeState { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Setting> Settings { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExchangeTradePair> ExchangeTradePairs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TradeAccount> TradeAccounts { get; set; }

        // Persistence models shouldn't have these two properties. To be honest its really convenient to have these here!
        // These are intiated at start up. 

        public IExchangeTrader TradingIntegration { get; set; }
    }
}
