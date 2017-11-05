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

        [Key]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string ItemIdentifier { get; set; }

        public string Message { get; set; }

        public bool RequirementMet { get; set; }


        public int TradeOpportunityRequirementTypeId { get; set; }
        public int TradeOpportunityId { get; set; }

        [ForeignKey("TradeOpportunityRequirementTypeId")]
        public TradeOpportunityRequirementType TradeOpportunityRequirementType { get; set; }

        [ForeignKey("TradeOpportunityId")]
        public TradeOpportunity TradeOpportunity { get; set; }
    }
}
