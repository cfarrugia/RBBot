using RBBot.Core.Exchanges.Poloniex.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges.Poloniex
{
    public class Authenticator
    {
        private ApiWebClient ApiWebClient { get; set; }

        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }

        internal Authenticator(ApiWebClient apiWebClient, string publicKey, string privateKey) : this(apiWebClient)
        {
            PublicKey = publicKey;
            PrivateKey = privateKey;
            apiWebClient.Authenticator = this;
        }

        internal Authenticator(ApiWebClient apiWebClient)
        {
            ApiWebClient = apiWebClient;
        }
    }

}
