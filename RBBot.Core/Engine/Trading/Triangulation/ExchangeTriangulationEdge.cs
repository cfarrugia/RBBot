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
        public ExchangeTradePair CurrentPrice { get; set; }
        public bool IsReversed { get; set; }

        public string GenericDescription
        {
            get
            {
                var c1 = this.CurrentPrice.TradePair.FromCurrency.ToString();
                var c2 = this.CurrentPrice.TradePair.ToCurrency.ToString();
                return !IsReversed ? $"{c1} - {c2}" : $"{c2} - {c1}";
            }
        }

        public string DetailedDescription
        {
            get
            {
                var c1 = this.CurrentPrice.TradePair.FromCurrency.ToString();
                var c2 = this.CurrentPrice.TradePair.ToCurrency.ToString();
                return this.CurrentPrice.LatestPrice == 0m ? "0.00000" : !IsReversed ? $"{c1} - {c2} ({this.CurrentPrice.LatestPrice:0.00000})" : $"{c2} - {c1}({(1 / this.CurrentPrice.LatestPrice):0.00000})";
            }
        }

        public override string ToString()
        {
            return DetailedDescription;
        }

    }
}
