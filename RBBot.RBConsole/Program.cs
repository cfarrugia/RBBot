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

        public static void Main(string[] args)
        {


            var r = new Random();


            IObservable<PriceChangeEvent> source0 = Observable.Generate<int, PriceChangeEvent>(0, i => i < 5000, i => i + 1, i => new PriceChangeEvent() { Price = Convert.ToDecimal(r.NextDouble()) }, i => TimeSpan.FromSeconds(1));
            IObservable<PriceChangeEvent> source1 = Observable.Generate<int, PriceChangeEvent>(0, i => i < 5000, i => i + 1, i => new PriceChangeEvent() { Price = Convert.ToDecimal(r.NextDouble()) }, i => TimeSpan.FromSeconds(1));
            IObservable<PriceChangeEvent> source2 = Observable.Generate<int, PriceChangeEvent>(0, i => i < 5000, i => i + 1, i => new PriceChangeEvent() { Price = Convert.ToDecimal(r.NextDouble()) }, i => TimeSpan.FromSeconds(1));
            IObservable<PriceChangeEvent> source3 = Observable.Generate<int, PriceChangeEvent>(0, i => i < 5000, i => i + 1, i => new PriceChangeEvent() { Price = Convert.ToDecimal(r.NextDouble()) }, i => TimeSpan.FromSeconds(1));
            IObservable<PriceChangeEvent> source = source0.Merge(source1).Merge(source2).Merge(source3);



            IDisposable subscription = source.Where(x => x.Price > 0.5m).Select(x => x.Price).Subscribe((x) => Console.WriteLine($"next over .5: {x}"), (ex) => Console.WriteLine($"error occurred: {ex.Message}"), () => Console.WriteLine("Completed!"));
            IDisposable subscription2 = source.Where(x => x.Price < 0.5m).Select(x => x.Price).Subscribe((x) => Console.WriteLine($"next under .5: {x}"), (ex) => Console.WriteLine($"error occurred: {ex.Message}"), () => Console.WriteLine("Completed!"));

            Console.ReadLine();

                return;



            try
            {
                Task.Run(async () =>
                {
                        // We define the list of observers here.
                        var priceObservers = new List<IMarketPriceObserver>();
                    priceObservers.Add(MarketPriceRecorder.Instance);
                    //priceObservers.Add(ArbPriceManager.Instance);
                    priceObservers.Add(TriangulationManager.Instance);

                        // Initialize the data processing engine.
                        await RBBot.Core.Engine.DataProcessingEngine.InitializeEngine(priceObservers.ToArray());

                        // Initialize the opportunity scoring engine.
                        await RBBot.Core.Engine.OpportunityScoreEngine.InitializeEngine();

                        // Initialize the trading engine.

                    }).GetAwaiter().GetResult();


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.ReadLine();

        }
    }

}

