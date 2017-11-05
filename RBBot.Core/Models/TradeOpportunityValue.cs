namespace RBBot.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TradeOpportunityValue")]
    public partial class TradeOpportunityValue
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TradeOpportunityValue()
        {
        }

        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public decimal PotentialMargin { get; set; }

        public int TradeOpportunityStateId { get; set; }
        public int TradeOpportunityId { get; set; }

        [ForeignKey("TradeOpportunityStateId")]
        public virtual TradeOpportunityState TradeOpportunityState { get; set; }

        [ForeignKey("TradeOpportunityId")]
        public virtual TradeOpportunity TradeOpportunity { get; set; }
    }
}
