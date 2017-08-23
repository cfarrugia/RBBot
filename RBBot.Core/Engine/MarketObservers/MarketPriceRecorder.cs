using RBBot.Core.Database;
using RBBot.Core.Exchanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using RBBot.Core.Models;

namespace RBBot.Core.Engine.MarketObservers
{

    public class TradePairSpread
    {


        /// <summary>
        /// Think of the trade pair as the key of the object.
        /// </summary>
        TradePair TradePair { get; set; }

        /// <summary>
        /// Each trade pair stores a dictionary of exchanges and their current values.
        /// </summary>
        //Dictionary<Exchange, decimal>

        ExchangeTradePair MinimumValuedPair { get; set; }
        ExchangeTradePair MaximumValuedPair { get; set; }
        decimal MinimumPrice { get; set; }
        decimal MaximumPrice { get; set; }



        
    }

    /// <summary>
    /// On a price change, this market price recorder will just record to db asynchronously.
    /// </summary>
    public class MarketPriceRecorder : IMarketPriceObserver
    {

        private static volatile MarketPriceRecorder instance;
        private static object syncRoot = new Object();

        private MarketPriceRecorder() { }

        private ConcurrentDictionary<TradePair, TradePairSpread> spreadPerTradePair = new ConcurrentDictionary<TradePair, TradePairSpread>();

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
        
        /// <summary>
        /// On market price change this is called to persist the information to db.
        /// </summary>
        /// <param name="change"></param>
        public async Task OnMarketPriceChangeAsync(PriceChangeEvent change)
        {
            // Save to database.
            using (var ctx = new RBBotContext())
            {
                ctx.MarketPrice.Add(new Models.MarketPrice()
                {
                    ExchangeTradePairId = change.ExchangeTradePair.Id,
                    Price = change.Price,
                    Timestamp = change.UtcTime
                });

                // Write to console.
                Console.WriteLine($"Price Change on {change.ExchangeTradePair.Exchange.Name} for {change.ExchangeTradePair.TradePair.FromCurrency.Code} - {change.ExchangeTradePair.TradePair.ToCurrency.Code}. New Price: {change.Price}");
                
                await ctx.SaveChangesAsync();

            }
        }
    }
}
