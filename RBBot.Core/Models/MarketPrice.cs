namespace RBBot.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MarketPrice")]
    public partial class MarketPrice
    {
        public int Id { get; set; }

        public int ExchangeTradePairId { get; set; }

        public DateTime Timestamp { get; set; }

        public decimal Price { get; set; }

        public virtual ExchangeTradePair ExchangeTradePair { get; set; }
    }
}
