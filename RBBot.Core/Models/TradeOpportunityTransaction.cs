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

        public long Id { get; set; }

        public bool IsReal { get; set; }

        public int TradeOpportunityId { get; set; }

        public int FromAccountId { get; set; }

        public int ToAccountId { get; set; }

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

        public int ExecuteOnExchangeId { get; set; }

        public virtual Exchange ExecutedOnExchange { get; set; }

        public virtual TradeAccount FromAccount { get; set; }

        public virtual TradeAccount ToAccount { get; set; }
        public virtual TradeOpportunity TradeOpportunity { get; set; }
    }
}
