using RBBot.Core.Database;
using RBBot.Core.Exchanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using RBBot.Core.Models;

namespace RBBot.Core.Engine.Trading.Recorder
{



    /// <summary>
    /// On a price change, this market price recorder will just record to db asynchronously.
    /// </summary>
    public class MarketPriceRecorder : IMarketPriceObserver
    {

        #region Singleton Initialization

        private static volatile MarketPriceRecorder instance;
        private static object syncRoot = new Object();

        private MarketPriceRecorder() { }

        /// <summary>
        /// We just want one instance of the market price observer.
        /// </summary>
        public static MarketPriceRecorder Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MarketPriceRecorder();
                    }
                }

                return instance;
            }
        }

        #endregion


        /// <summary>
        /// On market price change this is called to persist the information to db.
        /// </summary>
        /// <param name="change"></param>
        public async Task OnMarketPriceChangeAsync(PriceChangeEvent change)
        {
            // Save to database.
            using (var ctx = new RBBotContext())
            {
                ctx.MarketPrices.Add(new Models.MarketPrice()
                {
                    ExchangeTradePairId = change.ExchangeTradePair.Id,
                    Price = change.Price,
                    Timestamp = change.UtcTime
                });

                // Write to console.
                //Console.WriteLine($"Price Change on {change.ExchangeTradePair.Exchange.Name} for {change.ExchangeTradePair.TradePair.FromCurrency.Code} - {change.ExchangeTradePair.TradePair.ToCurrency.Code}. New Price: {change.Price}");
                
                await ctx.SaveChangesAsync();

            }
        }
    }
}
