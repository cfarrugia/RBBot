using RBBot.Core.Database;
using RBBot.Core.Exchanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace RBBot.Core.Engine
{

    public class MarketPriceObserver : IPriceObserver
    {

        private static volatile MarketPriceObserver instance;
        private static object syncRoot = new Object();

        private MarketPriceObserver() { }

        /// <summary>
        /// We just want one instance of the market price observer.
        /// </summary>
        public static MarketPriceObserver Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MarketPriceObserver();
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
