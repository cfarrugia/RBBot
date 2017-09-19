namespace RBBot.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TradeOpportunityRequirement")]
    public partial class TradeOpportunityRequirement
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TradeOpportunityRequirement()
        {
        }

        public int Id { get; set; }

        public int TypeId { get; set; }

        public DateTime Timestamp { get; set; }

        public int TradeOpportunityTypeId { get; set; }

        public string Message { get; set; }

        public bool RequirementMet { get; set; }

        public TradeOpportunityRequirementType Type { get; set; }
        public TradeOpportunity TradeOpportunity { get; set; }
        public int TradeOpportunityId { get; set; }

    }
}
