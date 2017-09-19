using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RBBot.Core.Models;
using RBBot.Core.Helpers;
using RBBot.Core.Engine.Trading.Actions;
using RBBot.Core.Engine.Trading;

namespace RBBot.Core.Exchanges.Kraken
{
    /// <summary>
    /// Kraken is a little strange because on the one side we're using crypto compare to get the market values 
    /// but on the other end we're using this integration to trade
    /// </summary>
    public class KrakenIntegration : IExchangeTrader
    {

        Kraken.Common.KrakenClient krakenClient = null;
        private Dictionary<string, string> KrakenToInternalCurrencyCode = null;
        private Dictionary<string, string> InternalToKrakenCurrencyCode = null;


        public KrakenIntegration(Exchange exchange)
        {
            this.Exchange = exchange;
            this.krakenClient = new Common.KrakenClient(this.Exchange.GetSetting("ApiUrl"), this.Exchange.GetSetting("ApiKey"), this.Exchange.GetSetting("ApiSecret"));
        }

        public Exchange Exchange { get; private set; }

        public string Name {  get { return "Kraken";  } }

        internal string ConvertKrakenToInternalCurrencyCode(string currency, bool trimFirstCharacter = true)
        {
            // 
            currency = currency == "XBT" ? "BTC" : currency; // For some reason they call bitcoin XBT. We change it to BTC

            return currency;
        }

        public async Task<ExchangeBalance[]> GetBalancesAsync()
        {
            // Get all the currencies from kraken
            if (this.KrakenToInternalCurrencyCode == null)
            {
                // Get the full internal list.
                var fullInternalList = this.Exchange.ExchangeTradePairs.Select(x => x.TradePair.FromCurrency).Union(this.Exchange.ExchangeTradePairs.Select(x => x.TradePair.ToCurrency));
                var uniqueInternalList = fullInternalList.Where(x => x.IsCrypto).Select(x => x.Code).Distinct().ToList();


                this.KrakenToInternalCurrencyCode = (await this.krakenClient.GetAssetInfo()).Result.ToDictionary(x => x.Key, x => ConvertKrakenToInternalCurrencyCode(x.Value.AlternateName));
                this.InternalToKrakenCurrencyCode = this.KrakenToInternalCurrencyCode.ToDictionary(x => x.Value, x => x.Key);


                // Remove any keys that are not in the unique internal list.
                var keysToRemove = this.InternalToKrakenCurrencyCode.Keys.Except(uniqueInternalList).ToList();
                foreach (var key in keysToRemove) this.InternalToKrakenCurrencyCode.Remove(key);

                // copy back
                this.KrakenToInternalCurrencyCode = this.InternalToKrakenCurrencyCode.ToDictionary(x => x.Value, x => x.Key);
            }
            
            // Now get the current wallets we have available...
            var walletDict = (await krakenClient.GetAccountBalance()).Result.ToDictionary(x => ConvertKrakenToInternalCurrencyCode(x.Key), y => y.Value);

            // In this part, we check which balances don't have addresses already set. 
            var walletsWithAddr = this.Exchange.TradeAccounts.Where(x => x.Address != null && x.ExchangeIdentifier != null).ToDictionary(x => x.Currency.Code, x => x);


            // Get the deposit methods for each of the 
            var depositMehodDict = new Dictionary<string, string>();
            foreach (var key in this.KrakenToInternalCurrencyCode.Keys)
            {
                if (walletsWithAddr.ContainsKey(this.KrakenToInternalCurrencyCode[key]) && walletsWithAddr[this.KrakenToInternalCurrencyCode[key]].ExchangeIdentifier != null)
                    depositMehodDict.Add(key, walletsWithAddr[this.KrakenToInternalCurrencyCode[key]].ExchangeIdentifier);
                else
                    depositMehodDict.Add(key, (await krakenClient.GetDepositMethods(key)).Result[0].Method);
            }


            var depositMethodAddr = new Dictionary<string, string>();
            foreach (var key in this.KrakenToInternalCurrencyCode.Keys)
            {
                if (walletsWithAddr.ContainsKey(this.KrakenToInternalCurrencyCode[key]) && walletsWithAddr[this.KrakenToInternalCurrencyCode[key]].Address != null)
                    depositMethodAddr.Add(key, walletsWithAddr[this.KrakenToInternalCurrencyCode[key]].Address);
                else
                    depositMethodAddr.Add(key, (await krakenClient.GetDepositAddresses(key, depositMehodDict[key])).Result[0].Address);
            }


            // Take the intersection between the internal list and the list from kraken. this is our final list.
            return this.KrakenToInternalCurrencyCode.Keys.Select(x => 
                new ExchangeBalance(
                    this.Exchange, 
                    walletDict.ContainsKey(x) ? walletDict[x] : 0m, 
                    DateTime.UtcNow, 
                    this.KrakenToInternalCurrencyCode[x],
                    depositMethodAddr.ContainsKey(x) ? depositMethodAddr[x] : null,
                    depositMehodDict.ContainsKey(x) ? depositMehodDict[x] : null)
            ).ToArray();
        }

        public Task<string> GetDepositAddressAsync(Currency currency)
        {
            throw new NotImplementedException();
        }


        public Task WithdrawAsync(Currency currency, decimal amount, string fromAccountAddress, string toAccountAddress)
        {
            throw new NotImplementedException();
        }

        public Task<ExchangeOrderResponse> PlaceOrder(ExchangeOrderType orderType, decimal orderAmount, ExchangeTradePair tradePair)
        {
            throw new NotImplementedException();
        }

        public TransactionFee EstimateTransactionFee(ExchangeOrderType orderType, decimal orderAmount, ExchangeTradePair tradePair)
        {
            throw new NotImplementedException();
        }
    }
}
