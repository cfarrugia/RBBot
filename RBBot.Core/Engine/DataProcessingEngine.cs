using RBBot.Core.Database;
using RBBot.Core.Exchanges;
using RBBot.Core.Exchanges.Bitflyer;
using RBBot.Core.Exchanges.CryptoCompare;
using RBBot.Core.Exchanges.GDAX;
using RBBot.Core.Exchanges.OKCoin;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using RBBot.Core.Engine.Trading;

namespace RBBot.Core.Engine
{
    public static class DataProcessingEngine
    {
        public static async Task InitializeEngine(IMarketPriceObserver[] priceObservers)
        {


            IList<Exchange> exchangeModels = null;
            IList<ExchangeSetting> settings = null;

            // Get all exchangeModels. We need them to construct the integrations.
            using (var ctx = new RBBotContext())
            {
                exchangeModels = ctx.Exchanges
                    .Include(x => x.ExchangeState)
                    .Include(x => x.ExchangeSettings)
                    .Include(x => x.ExchangeTradePairs.Select(y => y.ExchangeTradePairState))
                    .Include(x => x.ExchangeTradePairs.Select(y => y.TradePair).Select(z => z.FromCurrency))
                    .Include(x => x.ExchangeTradePairs.Select(y => y.TradePair).Select(z => z.ToCurrency))
                    .Where(x => x.ExchangeState.Code != "OFF") // Don't get offline exchanges!
                    .ToList();




                    // Get all exchanges.
                foreach (var exchange in exchangeModels)
                {

                    foreach (var exTradePair in exchange.ExchangeTradePairs)
                    {

                        //System.Console.WriteLine($"Exchange: {exchange.Name}, TradePair: {exTradePair.TradePair.FromCurrency.Code} - {exTradePair.TradePair.ToCurrency.Code}");

                    }

                }

                settings = ctx.ExchangeSettings.ToList();
            }



            var ccExchangeIds = settings.Where(x => x.Name == "ReadFromCryptoCompare" && x.Value.ToLower() == "true").Select(x => x.ExchangeId).ToList();
            var ccExchanges = exchangeModels.Where(x => ccExchangeIds.Contains(x.Id)).ToList();

            List<ExchangeIntegration> integrations = new List<Exchanges.ExchangeIntegration>();

            integrations.Add(new CryptoCompareIntegration(priceObservers, ccExchanges.ToArray()));
            integrations.Add(new BitflyerIntegration(priceObservers, new[] { exchangeModels.Single(x => x.Name == "Bitflyer") }));
            integrations.Add(new GDAXIntegration(priceObservers, new[] { exchangeModels.Single(x => x.Name == "GDAX") }));
            integrations.Add(new OKCoinComIntegration(priceObservers, new[] { exchangeModels.Single(x => x.Name == "OKCoin.com") }));
            integrations.Add(new OKCoinCNIntegration(priceObservers, new[] { exchangeModels.Single(x => x.Name == "OKCoin.cn") }));
            //integrations.Add(new OKExIntegration(new[] { MarketPriceObserver.Instance }, new[] { exchangeModels.Single(x => x.Name == "OKEx")});

            foreach (var integration in integrations)
            {
                await integration.InitializeExchangePriceProcessingAsync();
            }

        }

    }
}
