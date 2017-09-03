using Newtonsoft.Json;
using RBBot.Core.Exchanges.Poloniex.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges.Poloniex.Wallet
{
    public class Deposit
    {
        [JsonProperty("currency")]
        public string Currency { get; private set; }
        [JsonProperty("address")]
        public string Address { get; private set; }
        [JsonProperty("amount")]
        public double Amount { get; private set; }

        [JsonProperty("timestamp")]
        private ulong TimeInternal
        {
            set { Time = Helper.UnixTimeStampToDateTime(value); }
        }
        public DateTime Time { get; private set; }
        [JsonProperty("txid")]
        public string TransactionId { get; private set; }
        [JsonProperty("confirmations")]
        public uint Confirmations { get; private set; }

        [JsonProperty("status")]
        public string Status { get; private set; }
    }
}
