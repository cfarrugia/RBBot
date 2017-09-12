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
        public List<ExchangeTriangulationEdge> Edges { get; set; } = new List<ExchangeTriangulationEdge>();


        private const int ExpirePriceAfterSeconds = 60;

        /// <summary>
        /// To get the value of this triangulation is to hope on each edge and reducing the fees.
        /// </summary>
        /// <returns></returns>
        public decimal GetValue()
        {
            decimal val = 1m;
            decimal fees = 0m;


            foreach (var edge in Edges)
            {
                // If price is zero or expired, return 0;
                if ((edge.CurrentPrice.Price <= 0m) || (edge.CurrentPrice.AgeMilliseconds > ExpirePriceAfterSeconds * 1000))
                {
                    return 0m; // Just return 0 mid-loop.
                }
                    
                val *= edge.IsReversed ? 1m / edge.CurrentPrice.Price : edge.CurrentPrice.Price; // Multiply or divide the value.
                fees += edge.CurrentPrice.ExchangeTradePair.FeePercent / 100m; // Reduce the fee percentage.
            }

            return val - fees;
        }

        public override string ToString()
        {
            return string.Join(" -> ", this.Edges);
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
