namespace RBBot.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TradeAccount")]
    public partial class TradeAccount
    {
        #region Object overrides
        public override bool Equals(object obj)
        {

            if (!(obj is TradeAccount)) return false;

            var tp = (TradeAccount)obj;

            return tp.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return this.Id;
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TradeAccount()
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ExchangeId { get; set; }

        public string ExchangeIdentifier { get; set; }

        public int CurrencyId { get; set; }

        public string Address { get; set; }

        public decimal Balance { get; set; }

        public DateTime LastUpdate { get; set; }

        public virtual Currency Currency { get; set; }

        public virtual Exchange Exchange { get; set; }
    }
}
