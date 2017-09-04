using Newtonsoft.Json;
using RBBot.Core.Exchanges.Kraken.Utils;

namespace RBBot.Core.Exchanges.Kraken.Common
{
    [JsonConverter(typeof(JArrayToObjectConverter))]
    public class Ohlc
    {
        /// <summary>
        /// Unix timestamp.
        /// </summary>
        public long Time { get; set; }

        public decimal Open { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Close { get; set; }

        /// <summary>
        /// Volume-weighted average price.
        /// </summary>
        public decimal Vwap { get; set; }

        public decimal Volume { get; set; }

        public int Count { get; set; }
    }
}
