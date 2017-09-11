using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading.Triangulation
{
    public class ExchangeTriangulation
    {
        public HashSet<ExchangeTriangulationEdge> Edges { get; set; }

        /// <summary>
        /// To get the value of this triangulation is to hope on each edge and reducing the fees.
        /// </summary>
        /// <returns></returns>
        public decimal GetValue()
        {
            decimal val = 0m;

            foreach (var edge in Edges)
            {
                val *= edge.IsReversed ? 1 / edge.CurrentPrice.Price : edge.CurrentPrice.Price; // Multiply or divide the value.
                val -= edge.CurrentPrice.ExchangeTradePair.FeePercent / 100m; // Reduce the fee percentage.
            }

            return val;
        }


        public decimal UpdatePriceAndGetValue(PriceChangeEvent e)
        {
            // Update edges;
            this.Edges.Where(x => x.CurrentPrice.ExchangeTradePair == e.ExchangeTradePair).ToList().ForEach(x =>
            {
                x.CurrentPrice.Price = e.Price;
                x.CurrentPrice.UtcLastUpdateTime = e.UtcTime;
            });

            return this.GetValue();
        }
    }
}
