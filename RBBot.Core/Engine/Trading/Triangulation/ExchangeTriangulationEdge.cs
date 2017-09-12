using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading.Triangulation
{
    public class ExchangeTriangulationEdge
    {
        public TradePairPrice CurrentPrice { get; set; }
        public bool IsReversed { get; set; }

        public override string ToString()
        {
            var c1 = this.CurrentPrice.ExchangeTradePair.TradePair.FromCurrency.ToString();
            var c2 = this.CurrentPrice.ExchangeTradePair.TradePair.ToCurrency.ToString();
            return !IsReversed ? $"{c1} - {c2} ({this.CurrentPrice.Price:0.00000})" : $"{c2} - {c1}({(1/this.CurrentPrice.Price):0.00000})";
        }

    }
}
