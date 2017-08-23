using System;
using System.Collections.Generic;

namespace RBBot.Core.Models
{
    public partial class Exchange
    {
        #region Object overrides
        public override bool Equals(object obj)
        {

            if (!(obj is Exchange)) return false;

            var tp = (Exchange)obj;

            return tp.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return this.Id;
        }

        public override string ToString()
        {
            return this.Name;
        }

        #endregion

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
