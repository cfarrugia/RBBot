using RBBot.Core.Helpers;
using RBBot.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RBBot.Core.Helpers;

namespace RBBot.Core.Engine.Trading.Triangulation
{
    /// <summary>
    /// This is a pretty simple observer. It keeps track of the current market price for each exchange pair and seeks any possible triangulation within the same exchange
    /// 
    /// </summary>
    public class TriangulationManager : IMarketPriceObserver
    {
        #region Singleton initialization

        private static volatile TriangulationManager instance;
        private static object syncRoot = new Object();

        private TriangulationManager() { }

        /// <summary>
        /// We just want one instance of the market price observer.
        /// </summary>
        public static TriangulationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new TriangulationManager();
                    }
                }

                return instance;
            }
        }

        #endregion


        /// <summary>
        /// Stores all the cycles in an exchange
        /// </summary>
        private ConcurrentDictionary<Exchange, ExchangeTriangulation[]> tradePairCycles = null;

        public Task OnMarketPriceChangeAsync(PriceChangeEvent change)
        {
            // First time we receive a price, we generate 
            ExchangeTriangulation[] triangulations = this.tradePairCycles.GetOrAdd(change.ExchangeTradePair.Exchange, GetTriangulationsForExchange(change.ExchangeTradePair.Exchange));

            // 
            foreach (var tria in triangulations)
            {
                if (tria.UpdatePriceAndGetValue(change) > 1)
                {
                    Console.WriteLine("Found a triangulation opportunity!!");
                }
            }

            return null;
        }

        public static ExchangeTriangulation[] GetTriangulationsForExchange(Exchange exchange)
        {
            // Loop through each tradepair. essentially, for each we need to check if the vertex already exists. if not, we create a dependency to and from. 
            Dictionary<Currency, Vertex<Currency>> dict = new Dictionary<Currency, Vertex<Currency>>();

            // Build all dependencies.
            foreach (var tp in exchange.ExchangeTradePairs)
            {

                Vertex<Currency> fromVertex = null;
                Vertex<Currency> ToVertex = null;

                if (!dict.ContainsKey(tp.TradePair.FromCurrency)) dict.Add(tp.TradePair.FromCurrency, new Vertex<Currency>(tp.TradePair.FromCurrency)) ;
                if (!dict.ContainsKey(tp.TradePair.ToCurrency)) dict.Add(tp.TradePair.ToCurrency, new Vertex<Currency>(tp.TradePair.ToCurrency));

                fromVertex = dict[tp.TradePair.FromCurrency];
                ToVertex = dict[tp.TradePair.ToCurrency];

                fromVertex.Dependencies.Add(ToVertex);
                ToVertex.Dependencies.Add(fromVertex);
            }

            // Now detect cycles. Excludes cycles of length 2. These are like ETH->BTC->ETH
            TarjanCycleDetectStack<Currency> tcds = new TarjanCycleDetectStack<Currency>();

            var cycles = tcds.DetectCycle(dict.Values.ToList()).ToList();//.Where(x => x.Count > 2).ToList();

            // From each cycle now generate the triangulation object!
            var triads = new List<ExchangeTriangulation>();
            foreach (var cycle in cycles)
            {
                var triad = new ExchangeTriangulation();

                for (int i = 0; i < cycle.Count - 1; i ++)
                {
                    // Get the trade pair representing currency[i] -> currency[i+1]
                    var fromCurrency = cycle[i];
                    var toCurrency = cycle[i + 1];

                    // Check if this is a forward edge.
                    var forwardPair = exchange.ExchangeTradePairs.SingleOrDefault(x => x.TradePair.FromCurrency == fromCurrency.Value && x.TradePair.ToCurrency == toCurrency.Value);
                    if (forwardPair != null)
                    {
                        triad.Edges.Add(new ExchangeTriangulationEdge() { IsReversed = false, CurrentPrice = new TradePairPrice() { ExchangeTradePair = forwardPair, Price = 1m, UtcLastUpdateTime = DateTime.UtcNow } });
                    }
                    else
                    {
                        var backwardPaid = exchange.ExchangeTradePairs.SingleOrDefault(x => x.TradePair.FromCurrency == toCurrency.Value && x.TradePair.ToCurrency == fromCurrency.Value);
                        triad.Edges.Add(new ExchangeTriangulationEdge() { IsReversed = true, CurrentPrice = new TradePairPrice() { ExchangeTradePair = backwardPaid, Price = 1m, UtcLastUpdateTime = DateTime.UtcNow } });
                    }

                }


                triads.Add(triad);
            }

            // return the triads.
            return triads.ToArray();
        
        }

    }
}
