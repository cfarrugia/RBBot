using PubNubMessaging.Core;
using RBBot.Core.Engine;
using RBBot.Core.Engine.Trading;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RBBot.Core.Helpers;
using BitFlyer.Apis;
using RBBot.Core.Engine.Trading.Actions;

namespace RBBot.Core.Exchanges.Bitflyer
{
    public class BitflyerIntegration : ExchangeIntegration, IExchangeTrader
    {
        public BitflyerIntegration(Exchange[] exchanges) : base(exchanges)
        {
            this.Exchange = exchanges[0];

            // Create the private bitflyer api. 
            this.bitflyerApiClient = new PrivateApi(this.Exchange.GetSetting("ApiKey"), this.Exchange.GetSetting("ApiSecret"));
            
        }


        public Exchange Exchange { get; private set; }


        public override string Name { get { return "Bitflyer";  } }

        private Dictionary<string, ExchangeTradePair> subsribedProducts { get; set; }

        private PrivateApi bitflyerApiClient = null;

        #region Price Reader part.
        
        private PubNubMessaging.Core.Pubnub pubnub { get; set; }

        public override async Task InitializeExchangePriceProcessingAsync()
        {

            // Bitflyer uses pubnub. This is a cloud based service which works with subscriptions. 
            pubnub = new Pubnub(null, this.Exchange.GetSetting("PubNubKey"));

            // Just in case you have problem, just remove these comments.
            //m.SetPubnubLog(new PubNubConsole());
            //m.SetInternalLogLevel(LoggingMethod.Level.Verbose);

            subsribedProducts  = this.tradingPairs.ToDictionary(x => "lightning_ticker_" + x.Value.TradePair.FromCurrency.Code + "_" + x.Value.TradePair.ToCurrency.Code, y=>y.Value);


            // Loop through each product and subscribe it.
            foreach (var product  in subsribedProducts)
            {
                pubnub.Subscribe<string>(product.Key, (data) =>
                {

                    try
                    {
                        // Get JSON array and extract product, time and last trade price.
                        dynamic dynObj = Newtonsoft.Json.Linq.JArray.Parse(data);
                        string productName = dynObj[0].product_code.Value;
                        DateTime utcTime = dynObj[0].timestamp.Value;
                        double lasttradeprice = dynObj[0].ltp.Value;

                        Task.Run(() => this.NotifyObserverOfPriceChange(product.Value, (decimal)lasttradeprice, utcTime));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    
                    //System.Console.WriteLine($"product: {product} / ltp: {lasttradeprice} / time : {utcTime} ");

                }, (data) => { }, (err) => System.Console.WriteLine(err.Description));

            }

            // We're not doing async. We just want the warning to be gone.
            await Task.Run(() => { });
            
        }



        public override async Task ShutdownExchangePriceProcessingDownAsync()
        {
            await Task.Run(() =>
            {
                foreach (var product in subsribedProducts)
                    pubnub.Unsubscribe(product.Key, (obj) => { }, (obj) => { }, (obj) => { }, (obj) => { });
            });
        }

        #endregion


        private static string FromCurrencyEnum(BitFlyer.Apis.CurrencyCode currency)
        {
            switch (currency)
            {
                case CurrencyCode.Btc :  return "BTC";
                case CurrencyCode.Eth: return "ETH";
                case CurrencyCode.Jpy: return "JPY";
                case CurrencyCode.Bch: return "BCH";
                //case CurrencyCode.Etc: return "ETC";
                case CurrencyCode.Ltc: return "LTC";
                //case CurrencyCode.Mona: return "MONA";
                default: return null; // This version of the apis doesn't support more currencies.
            }

        }


        private static CurrencyCode CurrencyCodeToEnum(string code)
        {
            switch (code)
            {
                case "BTC": return CurrencyCode.Btc;
                case "ETH": return CurrencyCode.Eth;
                case "JPY": return CurrencyCode.Jpy;
                case "BCH": return CurrencyCode.Bch;
                //case "ETC": return CurrencyCode.Etc;
                case "LTC": return CurrencyCode.Ltc;
                //case "MONA": return CurrencyCode.Mona;
                default: throw new Exception($"Bitflyer API: Unsupported currency: {code}"); // This version of the apis doesn't support more currencies.
            }

        }

        public async Task<ExchangeBalance[]> GetBalancesAsync()
        {
            // Get the gdax client and list all accounts.
            var addresses = await bitflyerApiClient.GetAddresses();
            var balances = await bitflyerApiClient.GetBalance();

            // Transform to exchange balance objects.
            var exchangeBalances = balances.Select(x => new ExchangeBalance(this.Exchange, Convert.ToDecimal(x.Available), DateTime.UtcNow, FromCurrencyEnum(x.CurrencyCode), null, null)).Where(x => x.CurrencyCode != null).ToArray();

            // update any addresses
            foreach (var addr in addresses)
            {
                var exchangeBalance = exchangeBalances.SingleOrDefault(x => x.CurrencyCode == FromCurrencyEnum(addr.CurrencyCode));
                if (exchangeBalance == null) continue;
                exchangeBalance.Address = addr.Address;
            }

            return exchangeBalances;
        }


        public Task WithdrawAsync(Currency currency, decimal amount, string fromAccountAddress, string toAccountAddress)
        {
            SendCoinParameter param = new SendCoinParameter();
            param.Address = toAccountAddress;
            param.Amount = Convert.ToDouble(amount);
            param.CurrencyCode = CurrencyCodeToEnum(currency.Code);



            //this.bitflyerApiClient.SendCoin(param);

            //WithdrawParameter param = new WithdrawParameter();
            //param.

            //this.bitflyerApiClient.depoWithdraw(null);
            throw new NotImplementedException();
        }

        public Task<ExchangeOrderResponse> PlaceOrder(ExchangeOrderType orderType, decimal orderAmount, Currency orderAmountCurrency, ExchangeTradePair tradePair)
        {
            throw new NotImplementedException();
        }

        public TransactionFee EstimateTransactionFee(ExchangeOrderType orderType, decimal orderAmount, Currency orderAmountCurrency, ExchangeTradePair tradePair)
        {
#warning watch out ... this was copied and pasted
            // Check what's the preferred currency (BTC) and take 0.25% of it. 
            var fee = (orderAmountCurrency == SystemSetting.PreferredCyptoCurrency ? 1.0m : tradePair.LatestPrice) * orderAmount * tradePair.FeePercent / 100m;
            return new TransactionFee() { Amount = fee, Currency = SystemSetting.PreferredCyptoCurrency };
        }
    }

    ///// <summary>
    ///// Use this class to log to console.
    ///// </summary>
    //public class PubNubConsole : IPubNubLog
    //{
    //    public LoggingMethod.Level LogLevel { get; set; }

    //    public void WriteToLog(string logText)
    //    {
    //        Console.WriteLine($"Biflyer message: {logText}");
    //    }
    //}

}
