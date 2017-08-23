using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RBBot.Core.Database;
using RBBot.Core.Engine.MarketObservers;
using RBBot.Core.Exchanges;
using RBBot.Core.Exchanges.Bitflyer;
using RBBot.Core.Exchanges.CryptoCompare;
using RBBot.Core.Exchanges.GDAX;
using RBBot.Core.Exchanges.OKCoin;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine
{
    public static class DefaultEngine
    {
        public static async void InitializeEngine()
        {


            IList<Exchange> exchangeModels = null;
            IList<ExchangeSetting> settings = null;

            // Get all exchangeModels. We need them to construct the integrations.
            using (var ctx = new RBBotContext())
            {
                exchangeModels = ctx.Exchange
                    .Include(x => x.Status)
                    .Include(x => x.ExchangeSetting)
                    .Include(x => x.ExchangeTradePair).ThenInclude(x => x.Status)
                    .Include(x => x.ExchangeTradePair).ThenInclude(y => y.TradePair).ThenInclude(x => x.FromCurrency)
                    .Include(x => x.ExchangeTradePair).ThenInclude(y => y.TradePair).ThenInclude(x => x.ToCurrency)
                    .Where(x => x.Status.Code != "OFF") // Don't get offline exchanges!
                    .ToList();


                // Get all exchanges.
                foreach (var exchange in exchangeModels)
                {

                    foreach (var exTradePair in exchange.ExchangeTradePair)
                    {

                        //System.Console.WriteLine($"Exchange: {exchange.Name}, TradePair: {exTradePair.TradePair.FromCurrency.Code} - {exTradePair.TradePair.ToCurrency.Code}");

                    }

                }

                settings = ctx.ExchangeSetting.ToList();
            }


            var priceObservers = new List<IMarketPriceObserver>();
            priceObservers.Add(MarketPriceRecorder.Instance);
            priceObservers.Add(MarketPriceSpreadTracker.Instance);


            var ccExchangeIds = settings.Where(x => x.Name == "ReadFromCryptoCompare" && x.Value.ToLower() == "true").Select(x => x.ExchangeId).ToList();
            var ccExchanges = exchangeModels.Where(x => ccExchangeIds.Contains(x.Id)).ToList();

            List<ExchangeIntegration> integrations = new List<Exchanges.ExchangeIntegration>();

            integrations.Add(new CryptoCompareIntegration(priceObservers.ToArray(), ccExchanges.ToArray()));
            integrations.Add(new BitflyerIntegration(priceObservers.ToArray(), new[] { exchangeModels.Single(x => x.Name == "Bitflyer") }));
            integrations.Add(new GDAXIntegration(priceObservers.ToArray(), new[] { exchangeModels.Single(x => x.Name == "GDAX") }));
            integrations.Add(new OKCoinComIntegration(priceObservers.ToArray(), new[] { exchangeModels.Single(x => x.Name == "OKCoin.com") }));
            integrations.Add(new OKCoinCNIntegration(priceObservers.ToArray(), new[] { exchangeModels.Single(x => x.Name == "OKCoin.cn") }));
            //integrations.Add(new OKExIntegration(new[] { MarketPriceObserver.Instance }, new[] { exchangeModels.Single(x => x.Name == "OKEx")});

            foreach (var integration in integrations)
            {
                await integration.InitializeAsync();
            }
            



            //IList <ExchangeIntegration> integrations = new[] { new GDAXIntegration(new[] { MarketPriceObserver.Instance }, exchangeModels.Single(x => x.Name == "GDAX" ) };

            //foreach (var exIntegration in integrations)
            //{
            //    exIntegration.InitializeAsync();
            //}



        }

    }
}
