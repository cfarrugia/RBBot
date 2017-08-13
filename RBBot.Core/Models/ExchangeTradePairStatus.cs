using System;
using System.Collections.Generic;

namespace RBBot.Core.Models
{
    public partial class ExchangeTradePairStatus
    {
        public ExchangeTradePairStatus()
        {
            ExchangeTradePair = new HashSet<ExchangeTradePair>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public virtual ICollection<ExchangeTradePair> ExchangeTradePair { get; set; }
    }
}
