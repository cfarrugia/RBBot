using RBBot.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.MarketObservers
{
    /// <summary>
    /// This market price tracker maintains a list of all trading pairs, their latest price and 
    /// </summary>
    public class MarketPriceSpreadTracker : IMarketPriceObserver
    {
        #region Singleton initialization

        private static volatile MarketPriceSpreadTracker instance;
        private static object syncRoot = new Object();

        private MarketPriceSpreadTracker() { }
        
        /// <summary>
        /// We just want one instance of the market price observer.
        /// </summary>
        public static MarketPriceSpreadTracker Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MarketPriceSpreadTracker();
                    }
                }

                return instance;
            }
        }

        #endregion

        private ConcurrentDictionary<TradePair, TradePairSpread> spreadPerTradePair = new ConcurrentDictionary<TradePair, TradePairSpread>();


        public async Task OnMarketPriceChangeAsync(PriceChangeEvent change)
        {
            // Get trade pair spread. 
            TradePairSpread tpSpread = this.spreadPerTradePair.GetOrAdd(change.ExchangeTradePair.TradePair, new TradePairSpread(change.ExchangeTradePair.TradePair));

            await tpSpread.UpdatePrice(change.ExchangeTradePair, change.Price, change.UtcTime);

        }
    }
}
