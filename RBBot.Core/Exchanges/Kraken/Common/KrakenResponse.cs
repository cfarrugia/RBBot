using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace RBBot.Core.Exchanges.Kraken.Common
{
    /// <summary>
    /// Response from Kraken API.
    /// </summary>
    /// <typeparam name="T">Type of result.</typeparam>
    public class KrakenResponse<T>
    {
        /// <summary>
        /// Gets or sets errors of a request.
        /// </summary>
        [JsonProperty("error")]
        public ReadOnlyCollection<ErrorString> Errors { get; set; }

        /// <summary>
        /// Gets or sets the result of a request.
        /// </summary>
        public T Result { get; set; } // Nullable
    }
}
