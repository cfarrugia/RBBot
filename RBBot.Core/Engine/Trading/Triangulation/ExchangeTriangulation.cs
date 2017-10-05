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
                if ((edge.CurrentPrice.LatestPrice <= 0m) || (edge.CurrentPrice.LatestUpdate.AddSeconds(ExpirePriceAfterSeconds) < DateTime.UtcNow))
                {
                    return 0m; // Just return 0 mid-loop.
                }
                    
                val *= edge.IsReversed ? 1m / edge.CurrentPrice.LatestPrice : edge.CurrentPrice.LatestPrice; // Multiply or divide the value.
                fees += edge.CurrentPrice.FeePercent / 100m; // Reduce the fee percentage.
            }

            return val - 1m - fees;
        }

        public string GenericDescription { get { return this.Edges[0].CurrentPrice.Exchange + " - " + string.Join(" -> ", this.Edges.Select(x => x.GenericDescription)); } }
        public string DetailedDescription { get { return this.Edges[0].CurrentPrice.Exchange + " - " + string.Join(" -> ", this.Edges.Select(x => x.DetailedDescription)); } }

    }
}
