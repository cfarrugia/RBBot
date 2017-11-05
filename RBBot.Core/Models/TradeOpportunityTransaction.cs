namespace RBBot.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TradeOpportunityTransaction")]
    public partial class TradeOpportunityTransaction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TradeOpportunityTransaction()
        {
        }

        [Key]
        public long Id { get; set; }

        public bool IsReal { get; set; }

        public decimal FromAmount { get; set; }

        public decimal ToAmount { get; set; }

        public decimal FromAccountFee { get; set; }

        public decimal ToAccountFee { get; set; }

        public decimal FromAccountBalanceBeforeTx { get; set; }

        public decimal ToAccountBalanceBeforeTx { get; set; }

        public decimal ExchangeRate { get; set; }

        public decimal EstimatedFromAccountBalanceAfterTx { get; set; }

        public decimal EstimatedToAccountBalanceAfterTx { get; set; }

        public DateTime CreationDate { get; set; }

        public string ExternalTransactionId { get; set; }



        public virtual int ExecutedOnExchangeId { get; set; }

        public virtual int FromAccountId { get; set; }

        public virtual int ToAccountId { get; set; }
        public virtual int TradeOpportunityId { get; set; }


        [ForeignKey("ExecutedOnExchangeId")]
        public virtual Exchange ExecutedOnExchange { get; set; }

        [ForeignKey("FromAccountId")]
        public virtual TradeAccount FromAccount { get; set; }

        [ForeignKey("ToAccountId")]
        public virtual TradeAccount ToAccount { get; set; }

        [ForeignKey("TradeOpportunityId")]
        public virtual TradeOpportunity TradeOpportunity { get; set; }
    }
}
