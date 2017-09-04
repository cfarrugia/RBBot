using Newtonsoft.Json;
using RBBot.Core.Exchanges.Kraken.Utils;

namespace RBBot.Core.Exchanges.Kraken.Common
{
    [JsonConverter(typeof(JArrayToObjectConverter))]
    public class Spread
    {
        public long Time { get; set; }

        public decimal Bid { get; set; }

        public decimal Ask { get; set; }
    }
}