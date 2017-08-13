using System;
using System.Collections.Generic;

namespace RBBot.Core.Models
{
    public partial class ExchangeSetting
    {
        public int Id { get; set; }
        public int ExchangeId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public virtual Exchange Exchange { get; set; }
    }
}
