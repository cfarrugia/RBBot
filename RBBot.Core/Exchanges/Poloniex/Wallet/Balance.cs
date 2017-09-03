using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges.Poloniex.Wallet
{
    public class Balance
    {
        [JsonProperty("available")]
        public double QuoteAvailable { get; private set; }
        [JsonProperty("onOrders")]
        public double QuoteOnOrders { get; private set; }
        [JsonProperty("btcValue")]
        public double BitcoinValue { get; private set; }
    }
}
