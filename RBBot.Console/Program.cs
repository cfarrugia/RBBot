using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using Microsoft.Extensions.Configuration;
using RBBot.Core.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using RBBot.Core.Exchanges;
using RBBot.Core.Models;
using RBBot.Core.Exchanges.GDAX;
using RBBot.Core.Engine;

namespace RBBot.Console
{

    public class Program
    {

        



        public static void Main(string[] args)
        {
            // Build the db context 
            var configuration = new ConfigurationBuilder().Build();
            RBBot.Core.Database.RBBotContext.ConnectionString = "Data Source=localhost\\sql2016;Initial Catalog=RBBot;Integrated Security=True"; // ConfigurationExtensions.GetConnectionString(configuration, "RBBot");

            
            IList<Exchange> exchangeModels = null;
            
            // Get all exchangeModels. We need them to construct the integrations.
            using (var ctx = new RBBotContext())
            {
                exchangeModels = ctx.Exchange
                    .Include(x => x.ExchangeTradePair).ThenInclude(y => y.TradePair).ThenInclude(x => x.FromCurrency)
                    .Include(x => x.ExchangeTradePair).ThenInclude(y => y.TradePair).ThenInclude(x => x.ToCurrency).ToList();


                // Get all exchanges.
                foreach (var exchange in exchangeModels)
                {

                    foreach (var exTradePair in exchange.ExchangeTradePair)
                    {

                        System.Console.WriteLine($"Exchange: {exchange.Name}, TradePair: {exTradePair.TradePair.FromCurrency.Code} - {exTradePair.TradePair.ToCurrency.Code}");

                    }

                }

            }

            // Construct the integrations
            // No DI for now. Initialize list of exchanges.
            var g = new GDAXIntegration(new[] { MarketPriceObserver.Instance }, exchangeModels.Single(x => x.Name == "GDAX"));
            g.InitializeAsync().Wait();

            //IList <ExchangeIntegration> integrations = new[] { new GDAXIntegration(new[] { MarketPriceObserver.Instance }, exchangeModels.Single(x => x.Name == "GDAX" ) };

            //foreach (var exIntegration in integrations)
            //{
            //    exIntegration.InitializeAsync();
            //}




        }

    }

}

