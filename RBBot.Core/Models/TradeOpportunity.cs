namespace RBBot.Core.Models
{
    using Engine.Trading;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Threading;

    [Table("TradeOpportunity")]
    public partial class TradeOpportunity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TradeOpportunity()
        {
            TradeOpportunityTransactions = new HashSet<TradeOpportunityTransaction>();
            TradeOpportunityRequirements = new HashSet<TradeOpportunityRequirement>();
            TradeOpportunityValues = new HashSet<TradeOpportunityValue>();
            LockingSemaphore = new SemaphoreSlim(1); // Only one thread can lock!
        }

        public int Id { get; set; }

        public bool IsExecuted { get; set; } = false;

        public bool? IsSimulation { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public DateTime? ExecutedTime { get; set; }


        public int TradeOpportunityTypeId { get; set; }
        public int TradeOpportunityStateId { get; set; }
        public int CurrencyId { get; set; }


        [ForeignKey("TradeOpportunityTypeId")]
        public virtual TradeOpportunityType TradeOpportunityType { get; set; }

        [ForeignKey("TradeOpportunityStateId")]
        public virtual TradeOpportunityState TradeOpportunityState { get; set; }

        [ForeignKey("CurrencyId")]
        public virtual Currency Currency { get; set; }

        public virtual string Description { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TradeOpportunityTransaction> TradeOpportunityTransactions { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TradeOpportunityRequirement> TradeOpportunityRequirements { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TradeOpportunityValue> TradeOpportunityValues { get; set; }


        /// <summary>
        /// This is a non-mapped property storing the latest opportunity object.
        /// </summary>
        [NotMapped]
        public Opportunity LatestOpportunity { get; set; }

        [NotMapped]
        public DateTime LastestUpdate { get; set; }

        [NotMapped]
        public bool IsExecuting { get; set; } = false;

        [NotMapped]
        public bool IsDbExecutedWritten { get; set; } = false;


        [NotMapped]
        public SemaphoreSlim LockingSemaphore { get; private set;}

    }
}
