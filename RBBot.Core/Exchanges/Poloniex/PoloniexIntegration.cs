using Newtonsoft.Json;
using RBBot.Core.Engine;
using RBBot.Core.Engine.Trading;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using RBBot.Core.Helpers;

namespace RBBot.Core.Exchanges.Poloniex
{
    public class PoloniexIntegration : ExchangeIntegration, IExchangeTrader
    {
        public override string Name {  get { return "Poloniex";  } }

        public Exchange Exchange { get; private set; }

        private PoloniexApiClient poloniexClient = null;

        public PoloniexIntegration(IMarketPriceObserver[] priceObservers, Exchange[] exchanges) : base(priceObservers, exchanges)
        {
            this.Exchange = exchanges[0];
            this.poloniexClient = new PoloniexApiClient(this.Exchange.GetSetting("ApiUrl"), this.Exchange.GetSetting("ApiKey"), this.Exchange.GetSetting("ApiSecret"));
        }

        public override async Task InitializeExchangePriceProcessingAsync()
        {
            string nodeCode = @"
                 return function (options, cb) {
                    console.log('entered');
                    var autobahn = require('autobahn');
                    
                    var connection = new autobahn.Connection({
                        url: options.url,
                        realm: 'realm1'
                        });

                    connection.onopen = function(session)
                    {
                        session.subscribe('ticker', function(message, kwargs){
                                options.onMessage(message, function (error, result) {
                                    if (error) throw error;
                                    //console.log(result);
                                });
                                
                            });
                    }

                    connection.onclose = function()
                    {
                        console.log('Websocket connection closed');
                    }

                    connection.open();

                    cb();
                }
             ";


            var node = EdgeJs.Edge.Func(nodeCode);

            var onNodeMessage = (Func<object, Task<object>>)(async (message) =>
            {
                try
                {
                    var parts = (object[])message;

                    var currencyPair = parts[0].ToString();
                    var last = Convert.ToDecimal(parts[1]);
                    var lowestAsk = Convert.ToDecimal(parts[2]);
                    var highestBid = Convert.ToDecimal(parts[3]);
                    var percentChange = Convert.ToDecimal(parts[4]);
                    var baseVolume = Convert.ToDecimal(parts[5]);
                    var quoteVolume = Convert.ToDecimal(parts[6]);
                    var isFrozen = parts[7].ToString();
                    var _24hrHigh = Convert.ToDecimal(parts[8]);
                    var _24hrLow = Convert.ToDecimal(parts[9]);

                    var fromCurrency = currencyPair.Split('_')[1];
                    var toCurrency = currencyPair.Split('_')[0];
                    var key = this.GetPairKey(this.Exchange.Name, fromCurrency, toCurrency);

                    if (this.tradingPairs.ContainsKey(key))
                    {
                        await this.NotifyObserverOfPriceChange(new PriceChangeEvent()
                        {

                            ExchangeTradePair = this.tradingPairs[key],
                            Price = last,
                            UtcTime = DateTime.UtcNow
                        });

                        return message;


                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }


                return message; // message;
            });


            await node(new
            {
                url = this.Exchange.GetSetting("ApiWebsocketUrl"),
                onMessage = onNodeMessage
            });

        }

        public override async Task ShutdownExchangePriceProcessingDownAsync()
        {
            return;
        }


        public async Task<ExchangeBalance[]> GetBalancesAsync()
        {
            // Get the wallets from poloniex
            var wallets = await poloniexClient.Wallet.GetBalancesAsync();
            var walletAddresses = await poloniexClient.Wallet.GetDepositAddressesAsync();


            // This will return an enormous list, and we don't want all of that. What we do is we just get 
            // the ones that are in the trading pairs of this exchange.
            var fullList = this.Exchange.ExchangeTradePairs.Select(x => x.TradePair.FromCurrency).Union(this.Exchange.ExchangeTradePairs.Select(x => x.TradePair.ToCurrency));
            var uniqueList = fullList.Where(x => x.IsCrypto).Select(x => x.Code).Distinct();


            return wallets
                .Where(x => uniqueList.Contains(x.Key))
                .Select(w => new ExchangeBalance(this.Exchange, Convert.ToDecimal(w.Value.QuoteAvailable), DateTime.UtcNow, w.Key, walletAddresses.ContainsKey(w.Key) ? walletAddresses[w.Key] : null, null))
                .ToArray();
        }

        public Task<string> GetDepositAddressAsync(Currency currency)
        {
            
            throw new NotImplementedException();
        }

        public Task WithdrawAsync(Currency currency, decimal amount, string fromAccountAddress, string toAccountAddress)
        {
            throw new NotImplementedException();
        }

        public Task PlaceOrder()
        {
            throw new NotImplementedException();
        }
    }
}
