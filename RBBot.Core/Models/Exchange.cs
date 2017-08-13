using System;
using System.Collections.Generic;

namespace RBBot.Core.Models
{
    public partial class Exchange
    {
        public Exchange()
        {
            ExchangeSetting = new HashSet<ExchangeSetting>();
            ExchangeTradePair = new HashSet<ExchangeTradePair>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int StatusId { get; set; }

        public virtual ICollection<ExchangeSetting> ExchangeSetting { get; set; }
        public virtual ICollection<ExchangeTradePair> ExchangeTradePair { get; set; }
        public virtual ExchangeStatus Status { get; set; }
    }
}
