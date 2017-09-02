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
        [Key]
        [Column(Order = 0)]
        public long Id { get; set; }

        [Key]
        [Column(Order = 1)]
        public bool IsReal { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TradeOpportunityId { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FromAccountId { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ToAccountId { get; set; }

        [Key]
        [Column(Order = 5)]
        public decimal FromAmount { get; set; }

        [Key]
        [Column(Order = 6)]
        public decimal ToAmount { get; set; }

        [Key]
        [Column(Order = 7)]
        public decimal FromAccountFee { get; set; }

        [Key]
        [Column(Order = 8)]
        public decimal ToAccountFee { get; set; }

        [Key]
        [Column(Order = 9)]
        public decimal FromAccountBalanceBeforeTx { get; set; }

        [Key]
        [Column(Order = 10)]
        public decimal ToAccountBalanceBeforeTx { get; set; }

        [Key]
        [Column(Order = 11)]
        public decimal ExchangeRate { get; set; }

        [Key]
        [Column(Order = 12)]
        public decimal EstimatedFromAccountBalanceAfterTx { get; set; }

        [Key]
        [Column(Order = 13)]
        public decimal EstimatedToAccountBalanceAfterTx { get; set; }

        [Key]
        [Column(Order = 14)]
        public DateTime CreationDate { get; set; }

        [Key]
        [Column(Order = 15)]
        public string ExternalTransactionId { get; set; }

        [Key]
        [Column(Order = 16)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ExecuteOnExchangeId { get; set; }

        public virtual Exchange Exchange { get; set; }

        public virtual TradeAccount TradeAccount { get; set; }

        public virtual TradeAccount TradeAccount1 { get; set; }

        public virtual TradeOpportunity TradeOpportunity { get; set; }
    }
}
