using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RBBot.Core.Engine;
using System.Net.WebSockets;
using System.Threading;
using Newtonsoft.Json;
using RBBot.Core.Models;
using RBBot.Core.Engine.Trading;
using Gdax;
using Gdax.Accounts;
using RBBot.Core.Helpers;
using Gdax.Authentication;
using Gdax.Products;
using System.Net;
using System.Reflection;
using System.Net.Configuration;
using RBBot.Core.Engine.Trading.Actions;

namespace RBBot.Core.Exchanges.GDAX
{
    public class GDAXIntegration : ExchangeIntegration, IExchangeTrader
    {
        
        public override string Name { get { return "GDAX"; } }

        public Exchange Exchange { get; private set; }

        private ClientWebSocket websocket = null;

        private GdaxClient gdaxClient = null;

        public GDAXIntegration(IMarketPriceObserver[] priceObservers, Exchange[] exchanges) : base(priceObservers, exchanges)
        {
            this.Exchange = exchanges[0];
            GdaxAuthenticator auth = new GdaxAuthenticator(this.Exchange.GetSetting("ApiKey"), this.Exchange.GetSetting("ApiPassPhrase"), this.Exchange.GetSetting("ApiSecret"));
            gdaxClient = new GdaxClient(auth);
        }

        public override async Task InitializeExchangePriceProcessingAsync()
        {
            // Get all the products from the trading pairs.
            string products = string.Join(",", this.tradingPairs.ToList().Select(x => "\"" + x.Value.TradePair.FromCurrency.Code + "-" + x.Value.TradePair.ToCurrency.Code + "\""));

            // Form the initiation string
            var text = @"{ ""type"": ""subscribe"", ""product_ids"": [" + products + "] }";

            var uri = this.Exchange.GetSetting("ApiWebsocketUrl");

            // And initialize the websocket, pasing ParseResult as the callback to parse data from GDAX
            this.websocket = await Helpers.WebSocketManager.Initialize(uri, new[] { text }, ParseResult);

        }

        public override async Task ShutdownExchangePriceProcessingDownAsync()
        {
            await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }
        

        // Callback when gdax send information.
        private async Task ParseResult(string result)
        {
            try
            {


                // We just want matches. Discard everything else.
                if (result.Contains("match"))
                {
                    var jsonObj = JsonConvert.DeserializeObject<GDAXTradeMatchJson>(result);



                    PriceChangeEvent priceChange = new PriceChangeEvent()
                    {
                        Price = jsonObj.price,
                        UtcTime = DateTime.Parse(jsonObj.time).ToUniversalTime(),
                        ExchangeTradePair = this.GetExchangeTradePair(this.tradingPairs.Values.First().Exchange.Name, jsonObj.product_id.Split('-')[0], jsonObj.product_id.Split('-')[1])

                    };

                    // Notify the observers!
                    await this.NotifyObserverOfPriceChange(priceChange);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }


        public async Task<ExchangeBalance[]> GetBalancesAsync()
        {
            // Get the gdax client and list all accounts.
            var accounts = await gdaxClient.ListAccountsAsync();


            // From the GDAX apis we never get the address. We manually input them in our database!
            // Transform to exchange balance objects.
            return accounts.ToList().Select(x => new ExchangeBalance(this.Exchange, x.Available, DateTime.UtcNow, x.Currency, null, x.Id.ToString())).ToArray();

            //Console.WriteLine($"GDAX Account for {x.Currency} has a balance of {x.DefaultAmount} and funded {x.FundedAmount} and holds {x.Holds}");
        }
        
        /// <summary>
        /// Withdraws money from an account in GDAX, sending it to another account address.
        /// </summary>
        /// <remarks>Note that we only implement withdraw and not deposit. If you think about it, we just need withdrawal and not deposit. To deposit INTO gdax, what 
        /// we would do is the other way round: we would withdraw from another exchange!</remarks>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <param name="fromAccountAddress"></param>
        /// <param name="toAccountAddress"></param>
        /// <returns></returns>
        public Task WithdrawAsync(Currency currency, decimal amount, string fromAccountAddress, string toAccountAddress)
        {
            
            throw new NotImplementedException();
        }

        public Task<string> GetDepositAddressAsync(Currency currency)
        {
            // For security reasons GDAX
            throw new NotImplementedException();
        }

        public Task<ExchangeOrderResponse> PlaceOrder(ExchangeOrderType orderType, decimal orderAmount, ExchangeTradePair tradePair)
        {
            throw new NotImplementedException();
        }

        public TransactionFee EstimateTransactionFee(ExchangeOrderType orderType, decimal orderAmount, ExchangeTradePair tradePair)
        {
            // GDAX uses the "from" pair to calculate fees
            // Otherwise we need to convert first. The is equal to the order amount * fee percent.
            var fee = orderAmount * tradePair.FeePercent;

            return new TransactionFee() { Amount = fee, Currency = tradePair.TradePair.FromCurrency };
        }
    }
}
