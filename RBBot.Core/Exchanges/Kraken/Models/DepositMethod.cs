using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges.Kraken.Models
{
    public class DepositMethod
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("limit")]
        public string Limit { get; set; }


        [JsonProperty("fee")]
        public string Fee { get; set; }


        [JsonProperty("address-setup-fee")]
        public string AddressSetupFee{ get; set; }
        
    }
}
