using RBBot.Core.Engine;
using RBBot.Core.Engine.Trading;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges.CryptoCompare
{
    public class CryptoCompareIntegration : ExchangeIntegration
    {
        public CryptoCompareIntegration(IMarketPriceProcessor[] priceObservers, Exchange[] ccExchanges) : base(priceObservers, ccExchanges)
        {
            
        }

        public override string Name 
        {
            get
            {
                return "CryptoCompare";
            }
        }


        public override async Task InitializeExchangePriceProcessingAsync()
        {

            var node = EdgeJs.Edge.Func(@"
                return function (options, cb) {

                    //console.log(options.subscriptions);                    

                    var io = require('socket.io-client');
                    var socket = io.connect(options.url);
                    var subscription = options.subscriptions;

                    socket.emit('SubAdd', {subs:subscription} );

                    socket.on('m', function(message){
                                options.onMessage(message, function (error, result) {
                                    if (error) throw error;
                                    //console.log(result);
                                });
                                
                            });

                    cb();

                    }
            ");

            var onNodeMessage = (Func<object, Task<object>>)(async (message) =>
            {
                try
                {
                    // The format of the response is as follows:
                    //{Type}~{ExchangeName}~{FromCurrency}~{ToCurrency}~{Flag}~{Price}~{LastUpdate}~{LastVolume}~{LastVolumeTo}~{LastTradeId}~{Volume24h}~{Volume24hTo}~{MaskInt}
                    string[] splitMessage = message.ToString().Split('~');

                    if (splitMessage[0] != "3" && splitMessage.Length >= 7) // Make sure the message we got is a price change and not any echo or other message
                    {
                        string exchange = splitMessage[1];
                        string fromCurrency = splitMessage[2];
                        string toCurrency = splitMessage[3];
                        decimal price = Convert.ToDecimal(splitMessage[5]);
                        DateTime lastUpdate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(splitMessage[6])).UtcDateTime; // It appears they use unix time. ;

                        await this.NotifyObserverOfPriceChange(new PriceChangeEvent()
                        {

                            ExchangeTradePair = this.tradingPairs[GetPairKey(exchange, fromCurrency, toCurrency)],
                            Price = price,
                            UtcTime = lastUpdate
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
               

                return message;
            });

#warning still hardcoded.
            await node(new
            {
                url = "https://streamer.cryptocompare.com/",
                // Format of subscription is '2~Exchange~BTC~USD'
                subscriptions = this.tradingPairs.Keys.Select(x => "2~" + x.Replace("-", "~")).ToArray(), 
                onMessage = onNodeMessage
            });
        }




       

        public override Task ShutdownExchangePriceProcessingDownAsync()
        {
            // Not sure how to switch off node gracefully
            return null;
        }
    }
}
