using System;
using System.Collections.Generic;

namespace RBBot.Core.Models
{
    public partial class ExchangeStatus
    {
        public ExchangeStatus()
        {
            Exchange = new HashSet<Exchange>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public virtual ICollection<Exchange> Exchange { get; set; }
        public virtual ExchangeStatus IdNavigation { get; set; }
        public virtual ExchangeStatus InverseIdNavigation { get; set; }
    }
}
