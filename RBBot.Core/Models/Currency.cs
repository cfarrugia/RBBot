namespace RBBot.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Currency")]
    public partial class Currency
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Currency()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        [StringLength(5)]
        public string Code { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public bool IsCrypto { get; set; }

        public int AverageTransferTimeMinutes { get; set; }

        /// <summary>
        /// This is the daily average standard deviation and can be used to predict how bad a trade could go!
        /// http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:standard_deviation_volatility
        /// </summary>
        public decimal DailyVolatilityIndex { get; set; }

        /// <summary>
        /// The average fee paid to do the transfer denominated in this currency itself.
        /// </summary>
        public decimal AverageTransferFee { get; set; }

        /// <summary>
        /// We use this in simulation to give out the equivalent of X USDs to each account!
        /// </summary>
        public decimal ApproximateUSDValue{ get; set; }

        public override string ToString()
        {
            return this.Code;
        }

    }
}
