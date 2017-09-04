using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges.Kraken.Models
{
    public class TierInfo
    {
        public int Limit { get; set; }
        public TimeSpan DecreaseTime { get; set; }
    }
}
