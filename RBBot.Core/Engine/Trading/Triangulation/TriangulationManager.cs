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
    public class TriangulationManager : IMarketPriceProcessor
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
        private ConcurrentDictionary<Exchange, ExchangeTriangulation[]> tradePairCycles = new ConcurrentDictionary<Exchange, ExchangeTriangulation[]>();

        public async Task<IEnumerable<Opportunity>> OnMarketPriceChangeAsync(ExchangeTradePair change)
        {
            // First time we receive a price, we generate 
            ExchangeTriangulation[] triangulations = this.tradePairCycles.GetOrAdd(change.Exchange, GetTriangulationsForExchange(change.Exchange));

            // 
            List<Opportunity> opportunities = new List<Opportunity>();

            foreach (var tria in triangulations)
            {
                var opporunity = (tria.GetValue() - 1m);
                opportunities.Add(new TriangulationOpportunity(tria));
            }

            // return all opportunities
            return opportunities;
            
        }

        public static ExchangeTriangulation[] GetTriangulationsForExchange(Exchange exchange)
        {
            var cycleFinder = new CycleFinder(exchange);
            var cycles = cycleFinder.GetCycles();

            // From each cycle now generate the triangulation object!
            var triads = new List<ExchangeTriangulation>();
            foreach (var cycle in cycles)
            {
                var triad = new ExchangeTriangulation();

                for (int i = 0; i < cycle.Count; i ++)
                {
                    // Get the trade pair representing currency[i] -> currency[i+1]
                    var fromCurrency = cycle[i];
                    var toCurrency = cycle[(i + 1) % cycle.Count];

                    // Check if this is a forward edge.
                    try
                    {
                        var forwardPair = exchange.ExchangeTradePairs.SingleOrDefault(x => x.TradePair.FromCurrency.Id == fromCurrency.Id && x.TradePair.ToCurrency.Id == toCurrency.Id);
                        if (forwardPair != null)
                        {
                            triad.Edges.Add(new ExchangeTriangulationEdge() { IsReversed = false, CurrentPrice = forwardPair });
                        }
                        else
                        {

                            var backwardPaid = exchange.ExchangeTradePairs.Single(x => x.TradePair.FromCurrency.Id == toCurrency.Id && x.TradePair.ToCurrency.Id == fromCurrency.Id);
                            triad.Edges.Add(new ExchangeTriangulationEdge() { IsReversed = true, CurrentPrice = backwardPaid });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ReadLine();
                    }

                }


                triads.Add(triad);
            }

            // return the triads.
            return triads.ToArray();
        
        }

    }
}
