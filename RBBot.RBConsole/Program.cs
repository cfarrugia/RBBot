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
using System.Threading;
using System.Reactive.Concurrency;
using System.Reactive.PlatformServices;

namespace RBBot.RBConsole
{

    public class Program
    {

        public static IObservable<Opportunity> GetOpportunityObservable(IExchange[] exchangeIntegrations, IMarketPriceProcessor[] priceProcessors)
        {
            IScheduler s = System.Reactive.Concurrency.ThreadPoolScheduler.Instance;

            // Get all price observablea.
            List<IObservable<ExchangeTradePair>> priceChangers =
            exchangeIntegrations.Where(x => x is IExchangePriceReader)
            .Select(x => x as IExchangePriceReader)
            .Select(x => Observable.FromEvent<ExchangeTradePair>(y => x.OnPriceChangeHandler += y, y => x.OnPriceChangeHandler -= y).ObserveOn(s))
            .ToList();

            // Merge all observers together into one stream.
            var priceChangeObserver = Observable.Empty<ExchangeTradePair>();
            priceChangers.ForEach(x => priceChangeObserver = priceChangeObserver.Merge(x));


            // Create Observables of opportunities by taking each price processor and creating observables from them.
            var opportunityObserver =
                from p in priceChangeObserver // Take price changes
                from o in priceProcessors.Select(pp => Observable.FromAsync<IEnumerable<Opportunity>>(() => pp.OnMarketPriceChangeAsync(p))).Merge() // Pump them into price processors. Each will return a list of opportunities.
                from oo in o.ToObservable() // Take this list and make it observable, reducing it to observable<opportunity>
                where oo != null // No null opportunities of any sorts.
                select oo;

            // Return the observable of opportunities. Filter out null opportunities.
            return opportunityObserver;//.Where(x => x.GetValue() > 0m);
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

                    
                    var integrations = await DataProcessingEngine.InitializeEngine(priceProcessors.ToArray());

                    // Initialize the data processing engine. This returns the integrations and start the integrations engines to spit out prices.
                    OpportunityScoreEngine.InitializeEngine();


                    // From the integrations and price processors we get a stream of opportunities.
                    // Then get a stream of trade opportunities. 
                    IObservable<TradeOpportunity> tradeableStream =
                        GetOpportunityObservable(integrations, priceProcessors.ToArray())
                        .GetTradeOpportunityStream();
                    //.Where(x => x.LatestOpportunity.RequirementsMet && x.LatestOpportunity.MaximumPossibleTransactionAmount > 0m); 

                    //
                    IScheduler s = System.Reactive.Concurrency.ThreadPoolScheduler.Instance;


                    var sub = tradeableStream.SubscribeOn(s).Subscribe(
                        (opp) => 
                        {
                            var state = TradeOpportunityState.States.Where(x => x.Id == opp.TradeOpportunityStateId).Single().Code;
                            var type = TradeOpportunityType.Types.Where(x => x.Id == opp.TradeOpportunityTypeId).Single().Code;

                            Console.WriteLine($"Opp: {opp.LatestOpportunity.UniqueIdentifier} | {state} | {opp.LatestOpportunity.GetValue():0.00}");
                        }, 
                        (err) => 
                        {
                            Console.WriteLine($"err: {err.ToString()}");
                        } 
                    );
                    

                }).GetAwaiter().GetResult();

                Console.WriteLine("Press any key to exit");
                Console.ReadLine();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }


        }
    }

}

