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
using RBBot.Core.Helpers;
using RBBot.Core.Exchanges.Poloniex;
using RBBot.Core.Exchanges.Kraken;

namespace RBBot.Core.Engine
{
    public static class DataProcessingEngine
    {
        public static async Task InitializeEngine(IMarketPriceObserver[] priceObservers)
        {


            IList<Exchange> exchangeModels = null;
            IList<Setting> settings = null;
            List<IExchange> integrations = null;

            // Get all exchangeModels. We need them to construct the integrations.
            using (var ctx = new RBBotContext())
            {

                // Load the exchanges and all the stuff associated with them
                exchangeModels = ctx.Exchanges
                    .Include(x => x.ExchangeState)
                    .Include(x => x.Settings)
                    .Include(x => x.ExchangeTradePairs.Select(y => y.ExchangeTradePairState))
                    .Include(x => x.ExchangeTradePairs.Select(y => y.TradePair).Select(z => z.FromCurrency))
                    .Include(x => x.ExchangeTradePairs.Select(y => y.TradePair).Select(z => z.ToCurrency))
                    .Where(x => x.ExchangeState.Code != "OFF") // Don't get offline exchanges!
                    .ToList();

                // Get / cache the settings. 
                settings = ctx.Settings.ToList();

                // Now since I'm lazy, i first put setting unencrypted in db, and then encrypt them here. 
                if (settings.Where(x => x.IsEncrypted == false && x.ShouldBeEncrypted == true).Any(x => { x.EncryptSetting(); return true; }))
                    ctx.SaveChanges();

                // Now initialize the setting helper with all settings!
                SettingHelper.InitializeSettings(settings.ToArray());


                // Initialize all exchanges and their integrations.
                var ccExchangeIds = settings.Where(x => x.Name == "ReadFromCryptoCompare" && x.Value.ToLower() == "true").Select(x => x.ExchangeId).ToList();
                var ccExchanges = exchangeModels.Where(x => ccExchangeIds.Contains(x.Id)).ToList();

                integrations = new List<Exchanges.IExchange>();
                integrations.Add(new CryptoCompareIntegration(priceObservers, ccExchanges.ToArray()));
                integrations.Add(new BitflyerIntegration(priceObservers, new[] { exchangeModels.Single(x => x.Name == "Bitflyer") }));
                integrations.Add(new GDAXIntegration(priceObservers, new[] { exchangeModels.Single(x => x.Name == "GDAX") }));
                integrations.Add(new OKCoinComIntegration(priceObservers, new[] { exchangeModels.Single(x => x.Name == "OKCoin.com") }));
                integrations.Add(new OKCoinCNIntegration(priceObservers, new[] { exchangeModels.Single(x => x.Name == "OKCoin.cn") }));
                
                integrations.Add(new PoloniexIntegration(exchangeModels.Single(x => x.Name == "Poloniex")));
                integrations.Add(new KrakenIntegration(exchangeModels.Single(x => x.Name == "Kraken")));


                // Synchronize all the trading accounts.
                var tradingExchanges = integrations.Where(x => x is IExchangeTrader).Select(x => x as IExchangeTrader).ToList();
                foreach (var te in tradingExchanges) await SynchronizeAccounts(te, ctx);
                await ctx.SaveChangesAsync();
            }

            var readerExchanges = integrations.Where(x => x is IExchangePriceReader).Select(x => x as IExchangePriceReader).ToList();
            foreach (var e in readerExchanges) await e.InitializeExchangePriceProcessingAsync();
        }

        private static async Task SynchronizeAccounts(IExchangeTrader exchangeIntegration, RBBotContext dbContext)
        {
            Exchange exchangeModel = exchangeIntegration.Exchange;

            // Get the balances from the exchange integration
            var exchangebalances = (await exchangeIntegration.GetBalancesAsync()).ToDictionary(x => x.CurrencyCode, y => y);

            // Get the exchange's trading accounts.
            var existingAccounts = exchangeModel.TradeAccounts.ToDictionary(x => x.Currency.Code, y => y);

            // If the account exists already, then update. 
            existingAccounts.Where(x => exchangebalances.Keys.Contains(x.Key)).ToList().ForEach(x =>
            {
                var exCurr = exchangebalances[x.Key];
                var acc = existingAccounts[x.Key];
                acc.Balance = exCurr.Balance;
                acc.LastUpdate = exCurr.Timestamp;

                if (exCurr.ExchangeIdentifier != null) acc.ExchangeIdentifier = exCurr.ExchangeIdentifier;
                if (exCurr.Address != null) acc.Address = exCurr.Address;
            });

            // If the account doesn't exist, then create it. 
            exchangebalances.Keys.Where(x => !existingAccounts.Keys.Contains(x)).ToList().ForEach(x =>
            {
                var b = exchangebalances[x];
                TradeAccount acc = new TradeAccount()
                {
                    Address = b.Address,
                    Balance = b.Balance,
                    Exchange = exchangeModel,
                    ExchangeIdentifier = b.ExchangeIdentifier,
                    LastUpdate = b.Timestamp,
                    Currency = dbContext.Currencies.Where(c => c.Code == b.CurrencyCode).Single()
                };

                dbContext.TradeAccounts.Add(acc);
            });


        }

    }
}
