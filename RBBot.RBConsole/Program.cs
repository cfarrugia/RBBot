using RBBot.Core.Database;
using RBBot.Core.Engine;
using RBBot.Core.Engine.Trading;
using RBBot.Core.Engine.Trading.Arb;
using RBBot.Core.Engine.Trading.Recorder;
using RBBot.Core.Exchanges.CryptoCompare;
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
           
            try
            {
                Task.Run(async () =>
                {
                    // We define the list of observers here.
                    var priceObservers = new List<IMarketPriceObserver>();
                    priceObservers.Add(MarketPriceRecorder.Instance);
                    priceObservers.Add(ArbPriceManager.Instance);

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

