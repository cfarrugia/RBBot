﻿using RBBot.Core.Engine;
using RBBot.Core.Engine.Trading;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RBBot.Core.Exchanges
{
    public abstract class ExchangeIntegration : IExchangePriceReader
    {
        /// <summary>
        /// The constructor of the integration
        /// </summary>
        /// <param name="priceObservers">The list of price observers</param>
        /// <param name="exchange">The full exchange db object (with all children items included).</param>
        public ExchangeIntegration(Exchange[] exchanges)
        {
            tradingPairs = new Dictionary<string, Models.ExchangeTradePair>();

            foreach (var exchange in exchanges)
            {
                foreach (var pair in exchange.ExchangeTradePairs.Where(x => x.ExchangeTradePairState.Code != "OFF").ToList())
                {
                    if (pair.ExchangeTradePairState.Code == "OFF") continue; // Ignore offline pairs.
                    tradingPairs.Add(GetPairKey(exchange.Name, pair.TradePair.FromCurrency.Code, pair.TradePair.ToCurrency.Code), pair);
                }
            }
        }

        
        protected Dictionary<string, ExchangeTradePair> tradingPairs = null;

        public abstract string Name { get; }

        public event Action<ExchangeTradePair> OnPriceChangeHandler;

        public abstract Task InitializeExchangePriceProcessingAsync();

        public abstract Task ShutdownExchangePriceProcessingDownAsync();


        protected async Task NotifyObserverOfPriceChange(ExchangeTradePair changedPair, decimal newPrice, DateTime updateTime)
        {

            // Get the trade pair and update its price.
            changedPair.LatestPrice = newPrice;
            changedPair.LatestUpdate = updateTime;

            // If somebody registered to the pricechange event, then call it.
            if (this.OnPriceChangeHandler != null) this.OnPriceChangeHandler(changedPair);


        }

        protected string GetPairKey(string exchangeName, string fromCurrencyCode, string ToCurrencyCode)
        {
            return exchangeName + "-" + fromCurrencyCode + "-" + ToCurrencyCode;
        }

        /// <summary>
        /// Helper method to get the trading pair db object from currency codes.
        /// </summary>
        protected ExchangeTradePair GetExchangeTradePair(string exchange, string fromCurrencyCode, string toCurrencyCode)
        {
            return this.tradingPairs[GetPairKey(exchange, fromCurrencyCode, toCurrencyCode)];
        }
    }
}
