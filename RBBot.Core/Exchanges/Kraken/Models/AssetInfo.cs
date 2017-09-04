using Newtonsoft.Json;

namespace RBBot.Core.Exchanges.Kraken.Common
{
    public class AssetInfo
    {
        public const string AssetClassCurrency = "currency";

        /// <summary>
        /// Alternate name.
        /// </summary>
        [JsonProperty("altname")]
        public string AlternateName { get; set; }

        /// <summary>
        /// Asset class.
        /// </summary>
        [JsonProperty("aclass")]
        public string AssetClass { get; set; }

        /// <summary>
        /// Scaling decimal places for record keeping.
        /// </summary>
        public int Decimals { get; set; }

        /// <summary>
        /// Scaling decimal places for output display.
        /// </summary>
        public int DisplayDecimals { get; set; }
    }
}