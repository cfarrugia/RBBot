using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RBBot.Core.Engine
{
    /// <summary>
    /// This class defines the generic price change. 
    /// </summary>
    public class PriceChangeEvent
    {
        public ExchangeTradePair ExchangeTradePair { get; set; }
        public decimal Price { get; set; }
        public DateTime UtcTime { get; set; }
    }
}
