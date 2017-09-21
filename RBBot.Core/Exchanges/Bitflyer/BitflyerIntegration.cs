using PubNubMessaging.Core;
using RBBot.Core.Engine;
using RBBot.Core.Engine.Trading;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges.Bitflyer
{
    public class BitflyerIntegration : ExchangeIntegration
    {
        public BitflyerIntegration(IMarketPriceProcessor[] priceObservers, Exchange[] exchanges) : base(priceObservers, exchanges)
        {
        }

        public override string Name { get { return "Bitflyer";  } }

        private Dictionary<string, ExchangeTradePair> subsribedProducts { get; set; }
        private Pubnub pubnub { get; set; }

        public override async Task InitializeExchangePriceProcessingAsync()
        {
            // Bitflyer uses pubnub. This is a cloud based service which works with subscriptions. 
#warning This key needs to be saved
            pubnub = new Pubnub(null, "sub-c-52a9ab50-291b-11e5-baaa-0619f8945a4f");

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

                        Task.Run(() => this.NotifyObserverOfPriceChange(new PriceChangeEvent()
                        {
                            ExchangeTradePair = product.Value,
                            Price = (decimal)lasttradeprice,
                            UtcTime = utcTime
                        }));

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
    }

    /// <summary>
    /// Use this class to log to console.
    /// </summary>
    public class PubNubConsole : IPubnubLog
    {
        public LoggingMethod.Level LogLevel { get; set; }

        public void WriteToLog(string logText)
        {
            System.Console.WriteLine(logText);
        }
    }

}
