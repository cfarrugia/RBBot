using System;
using System.Collections.Generic;

namespace RBBot.Core.Models
{
    public partial class ExchangeTradePair
    {
        public ExchangeTradePair()
        {
            MarketPrice = new HashSet<MarketPrice>();
        }

        public int Id { get; set; }
        public int TradePairId { get; set; }
        public int ExchangeId { get; set; }
        public decimal FeePercent { get; set; }
        public int StatusId { get; set; }



        public virtual ICollection<MarketPrice> MarketPrice { get; set; }
        public virtual Exchange Exchange { get; set; }
        public virtual ExchangeTradePair IdNavigation { get; set; }
        public virtual ExchangeTradePair InverseIdNavigation { get; set; }
        public virtual ExchangeTradePairStatus Status { get; set; }
        public virtual TradePair TradePair { get; set; }
    }
}
