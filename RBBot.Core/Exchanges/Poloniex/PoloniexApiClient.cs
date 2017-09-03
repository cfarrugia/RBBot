using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using RBBot.Core.Exchanges.Poloniex.Common;
using System.Threading;

namespace RBBot.Core.Exchanges.Poloniex
{
    public sealed class PoloniexApiClient
    {
        /// <summary>Represents the authenticator object of the client.</summary>
        public Authenticator Authenticator { get; private set; }

        /// <summary>A class which contains market tools for the client.</summary>
        //public IMarkets Markets { get; private set; }
        ///// <summary>A class which contains trading tools for the client.</summary>
        //public ITrading Trading { get; private set; }
        /// <summary>A class which contains wallet tools for the client.</summary>
        public Wallet.Wallet Wallet { get; private set; }

        /// <summary>Creates a new instance of Poloniex API .NET's client service.</summary>
        /// <param name="publicApiKey">Your public API key.</param>
        /// <param name="privateApiKey">Your private API key.</param>
        public PoloniexApiClient(string apiAddress, string publicApiKey, string privateApiKey)
        {
            var apiWebClient = new ApiWebClient(apiAddress);

            Authenticator = new Authenticator(apiWebClient, publicApiKey, privateApiKey);

            //Markets = new Markets(apiWebClient);
            //Trading = new Trading(apiWebClient);
            Wallet = new Wallet.Wallet(apiWebClient);
        }

    }

}