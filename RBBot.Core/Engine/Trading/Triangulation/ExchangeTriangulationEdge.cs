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
        
    }
}
