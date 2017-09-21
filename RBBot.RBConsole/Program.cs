using RBBot.Core.Database;
using RBBot.Core.Engine;
using RBBot.Core.Engine.Trading;
using RBBot.Core.Engine.Trading.Arb;
using RBBot.Core.Engine.Trading.Recorder;
using RBBot.Core.Engine.Trading.Triangulation;
using RBBot.Core.Exchanges.CryptoCompare;
using RBBot.Core.Helpers;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using RBBot.Core.Exchanges;
using System.Reactive.Subjects;

namespace RBBot.RBConsole
{


    public class Program
    {

        public static async IObservable<Opportunity> GetOpportunityObservable(IExchange[] exchangeIntegrations, IMarketPriceProcessor[] priceProcessors)
        {
            // Get all price observablea.
            List<IObservable<PriceChangeEvent>> priceChangers =
            exchangeIntegrations.Where(x => x is IExchangePriceReader)
            .Select(x => x as IExchangePriceReader)
            .Select(x => Observable.FromEvent<PriceChangeEvent>(y => x.OnPriceChangeHandler += y, y => x.OnPriceChangeHandler -= y))
            .ToList();

            // Merge all observers together into one stream.
            var priceChangeObserver = Observable.Empty<PriceChangeEvent>();
            priceChangers.ForEach(x => priceChangeObserver = priceChangeObserver.Merge(x));


            // Create Observables of opportunities by taking each price processor and creating observables from them.
            var opportunityObserver =
                from p in priceChangeObserver // Take price changes
                from o in priceProcessors.Select(pp => Observable.FromAsync<IEnumerable<Opportunity>>(() => pp.OnMarketPriceChangeAsync(p))).Merge() // Pump them into price processors. Each will return a list of opportunities.
                from oo in o.ToObservable() // Take this list and make it observable, reducing it to observable<opportunity>
                where oo != null // No null opportunities of any sorts.
                select oo;

            // Return the observable of opportunities. Filter out null opportunities.
            return opportunityObserver;
        }



        public static void Main(string[] args)
        {

            

            try
            {
                Task.Run(async () =>
                {
                    // We define the list of observers here.
                    var priceProcessors = new List<IMarketPriceProcessor>();
                    priceProcessors.Add(MarketPriceRecorder.Instance);
                    priceProcessors.Add(ArbPriceManager.Instance);
                    priceProcessors.Add(TriangulationManager.Instance);

                    // Initialize the data processing engine. This returns the integrations and start the integrations engines to spit out prices.
                    var integrations = await RBBot.Core.Engine.DataProcessingEngine.InitializeEngine(priceProcessors.ToArray());

                    // From the integrations and price processors we get a stream of opportunities.
                    IObservable<Opportunity> opportunityStream = GetOpportunityObservable(integrations, priceProcessors.ToArray());

                    // These opportunities are stateless 

                    //var sub = priceChangeObserver.Subscribe((np) => { Console.WriteLine($"new price from {np.ExchangeTradePair.Exchange} for {np.ExchangeTradePair.TradePair} is {np.Price:0.0000}"); });

                    var sub = opportunityStream.Subscribe((opp) => { Console.WriteLine($"new opportunity!"); }, (err) => { Console.WriteLine($"err: {err.Message}"); } );


                    // Initialize the opportunity scoring engine.
                    //await RBBot.Core.Engine.OpportunityScoreEngine.InitializeEngine();

                    // Initialize the trading engine.

                }).GetAwaiter().GetResult();


                Console.ReadLine();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.ReadLine();

        }
    }

}

