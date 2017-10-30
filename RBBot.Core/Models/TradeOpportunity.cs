namespace RBBot.Core.Models
{
    using Engine.Trading;
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
            TradeOpportunityRequirements = new HashSet<TradeOpportunityRequirement>();
        }

        public int Id { get; set; }

        public int TradeOpportunityTypeId { get; set; }

        public int CurrencyId { get; set; }

        public bool IsExecuted { get; set; } = false;

        public bool? IsSimulation { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public DateTime? ExecutedTime { get; set; }

        public int TradeOpportunityStateId { get; set; }

        public virtual TradeOpportunityType TradeOpportunityType { get; set; }

        public virtual TradeOpportunityState TradeOpportunityState { get; set; }

        public virtual string Description { get; set; }

        public virtual Currency Currency{ get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TradeOpportunityTransaction> TradeOpportunityTransactions { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TradeOpportunityRequirement> TradeOpportunityRequirements { get; set; }

        /// <summary>
        /// This is a non-mapped property storing the latest opportunity object.
        /// </summary>
        [NotMapped]
        public Opportunity LatestOpportunity { get; set; }

        [NotMapped]
        public DateTime LastestUpdate { get; set; }

        [NotMapped]
        public bool IsExecuting { get; set; } = false;
        
    }
}
