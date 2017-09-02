namespace RBBot.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TradeOpportunity")]
    public partial class TradeOpportunity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TradeOpportunity()
        {
            TradeOpportunityTransactions = new HashSet<TradeOpportunityTransaction>();
            //TradeOpportunityValues = new HashSet<TradeOpportunityValue>();
        }

        public int Id { get; set; }

        public int TradeOpportunityTypeId { get; set; }

        public int CurrencyId { get; set; }

        public bool IsExecuted { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public DateTime? ExecutedTime { get; set; }

        public virtual TradeOpportunityType TradeOpportunityType { get; set; }

        public virtual Currency Currency{ get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TradeOpportunityTransaction> TradeOpportunityTransactions { get; set; }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<TradeOpportunityValue> TradeOpportunityValues { get; set; }
    }
}
