using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges.Kraken.Models
{
    public class DepositAddress
    {

        /// <summary>
        /// Address of account
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; set; }

        /// <summary>
        /// Unix timestamp of order end time (or 0 if not set).
        /// </summary>
        [JsonProperty("expiretm")]
        public double ExpireTime { get; set; }

        /// <summary>
        /// If this address is new or not.
        /// </summary>
        [JsonProperty("new")]
        public bool IsNew { get; set; }

    }

}
