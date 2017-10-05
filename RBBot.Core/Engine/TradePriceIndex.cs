using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine
{
    /// <summary>
    /// Think of this method as a index/helper/fast way of getting prices by different criteria.
    /// </summary>
    public static class TradePriceIndex
    {
        private static Dictionary<TradePair, HashSet<ExchangeTradePair>> indexByTradePair = new Dictionary<TradePair, HashSet<ExchangeTradePair>>();

        /// <summary>
        /// Initializes the class
        /// </summary>
        public static void Initialize(Exchange[] exchanges, TradePair[] tradePairs)
        {
            indexByTradePair = tradePairs.ToDictionary(x => x, x => new HashSet<ExchangeTradePair>());
            foreach (var ex in exchanges)
                foreach (var etp in ex.ExchangeTradePairs)
                    indexByTradePair[etp.TradePair].Add(etp);
        }

        /// <summary>
        /// From a generic trade pair, this method will return the list of tradepairs from all exchanges
        /// </summary>
        /// <param name="tradePair"></param>
        public static HashSet<ExchangeTradePair> GetExchangeTradePairs(TradePair tradePair)
        {
            return indexByTradePair[tradePair];
        }


    }
}
