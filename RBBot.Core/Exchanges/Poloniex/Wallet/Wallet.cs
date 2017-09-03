using RBBot.Core.Exchanges.Poloniex.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges.Poloniex.Wallet
{
    public class Wallet
    {
        private ApiWebClient ApiWebClient { get; set; }

        internal Wallet(ApiWebClient apiWebClient)
        {
            ApiWebClient = apiWebClient;
        }

        private IDictionary<string,Balance> GetBalances()
        {
            var postData = new Dictionary<string, object>();

            var data = PostData<IDictionary<string, Balance>>("returnCompleteBalances", postData);
            return data;
        }

        private IDictionary<string, string> GetDepositAddresses()
        {
            var postData = new Dictionary<string, object>();

            var data = PostData<IDictionary<string, string>>("returnDepositAddresses", postData);
            return data;
        }

        private DepositWithdrawalList GetDepositsAndWithdrawals(DateTime startTime, DateTime endTime)
        {
            var postData = new Dictionary<string, object> {
                { "start", Helper.DateTimeToUnixTimeStamp(startTime) },
                { "end", Helper.DateTimeToUnixTimeStamp(endTime) }
            };

            var data = PostData<DepositWithdrawalList>("returnDepositsWithdrawals", postData);
            return data;
        }

        private GeneratedDepositAddress PostGenerateNewDepositAddress(string currency)
        {
            var postData = new Dictionary<string, object> {
                { "currency", currency }
            };

            var data = PostData<GeneratedDepositAddress>("generateNewAddress", postData);
            return data;
        }

        private void PostWithdrawal(string currency, double amount, string address, string paymentId)
        {
            var postData = new Dictionary<string, object> {
                { "currency", currency },
                { "amount", amount.ToStringNormalized() },
                { "address", address }
            };

            if (paymentId != null)
            {
                postData.Add("paymentId", paymentId);
            }

            PostData<GeneratedDepositAddress>("withdraw", postData);
        }

        public Task<IDictionary<string, Balance>> GetBalancesAsync()
        {
            return Task.Factory.StartNew(() => GetBalances());
        }

        public Task<IDictionary<string, string>> GetDepositAddressesAsync()
        {
            return Task.Factory.StartNew(() => GetDepositAddresses());
        }

        public Task<DepositWithdrawalList> GetDepositsAndWithdrawalsAsync(DateTime startTime, DateTime endTime)
        {
            return Task.Factory.StartNew(() => GetDepositsAndWithdrawals(startTime, endTime));
        }

        public Task<DepositWithdrawalList> GetDepositsAndWithdrawalsAsync()
        {
            return Task.Factory.StartNew(() => GetDepositsAndWithdrawals(Helper.DateTimeUnixEpochStart, DateTime.MaxValue));
        }

        public Task<GeneratedDepositAddress> PostGenerateNewDepositAddressAsync(string currency)
        {
            return Task.Factory.StartNew(() => PostGenerateNewDepositAddress(currency));
        }

        public Task PostWithdrawalAsync(string currency, double amount, string address, string paymentId)
        {
            return Task.Factory.StartNew(() => PostWithdrawal(currency, amount, address, paymentId));
        }

        public Task PostWithdrawalAsync(string currency, double amount, string address)
        {
            return Task.Factory.StartNew(() => PostWithdrawal(currency, amount, address, null));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T PostData<T>(string command, Dictionary<string, object> postData)
        {
            return ApiWebClient.PostData<T>(command, postData);
        }
    }
}
