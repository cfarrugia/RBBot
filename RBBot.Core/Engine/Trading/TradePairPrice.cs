using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading
{
    public class TradePairPrice : IComparable<TradePairPrice>
    {
        public decimal Price { get; set; } = 0m;
        public DateTime UtcLastUpdateTime { get; set; } = DateTime.MinValue;
        public ExchangeTradePair ExchangeTradePair { get; set; }

        public int AgeMilliseconds { get { return (DateTime.UtcNow - this.UtcLastUpdateTime).Milliseconds; } }

        public int CompareTo(TradePairPrice other)
        {
            if (this.Price > other.Price)
                return 1;
            if (this.Price < other.Price)
                return -1;
            else
                return 0;
        }
    }

}
