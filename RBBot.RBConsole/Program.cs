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

namespace RBBot.RBConsole
{

    public class Program
    {

        

        public static void Main(string[] args)
        {
            // tests simple model presented on https://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
            var graph_nodes = new List<Vertex<int>>();

            var v1 = new Vertex<int>(1);
            var v2 = new Vertex<int>(2);
            var v3 = new Vertex<int>(3);
            var v4 = new Vertex<int>(4);
            var v5 = new Vertex<int>(5);
            var v6 = new Vertex<int>(6);
            var v7 = new Vertex<int>(7);
            var v8 = new Vertex<int>(8);

            v1.Dependencies.Add(v2);
            v2.Dependencies.Add(v3);
            v3.Dependencies.Add(v1);
            v4.Dependencies.Add(v3);
            v4.Dependencies.Add(v5);
            v5.Dependencies.Add(v4);
            v5.Dependencies.Add(v6);
            v6.Dependencies.Add(v3);
            v6.Dependencies.Add(v7);
            v7.Dependencies.Add(v6);
            v8.Dependencies.Add(v7);
            v8.Dependencies.Add(v5);
            v8.Dependencies.Add(v8);

            graph_nodes.Add(v1);
            graph_nodes.Add(v2);
            graph_nodes.Add(v3);
            graph_nodes.Add(v4);
            graph_nodes.Add(v5);
            graph_nodes.Add(v6);
            graph_nodes.Add(v7);
            graph_nodes.Add(v8);

            var tcd = new TarjanCycleDetectStack<int>();
            var cycle_list = tcd.DetectCycle(graph_nodes);


            try
                {
                    Task.Run(async () =>
                    {
                    // We define the list of observers here.
                    var priceObservers = new List<IMarketPriceObserver>();
                        priceObservers.Add(MarketPriceRecorder.Instance);
                        priceObservers.Add(ArbPriceManager.Instance);
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

