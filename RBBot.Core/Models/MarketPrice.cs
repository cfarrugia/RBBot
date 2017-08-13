using System;
using System.Collections.Generic;

namespace RBBot.Core.Models
{
    public partial class MarketPrice
    {
        public int Id { get; set; }
        public int ExchangeTradePairId { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Price { get; set; }

        public virtual ExchangeTradePair ExchangeTradePair { get; set; }
    }
}
