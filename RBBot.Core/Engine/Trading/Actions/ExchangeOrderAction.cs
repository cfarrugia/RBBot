using RBBot.Core.Exchanges;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading.Actions
{
    /// <summary>
    /// This is the class responsable for the manifestation of an order. 
    /// </summary>
    public class ExchangeOrderAction : ITradeAction
    {
        /// <summary>
        /// The trade pair for which the order will be executed.
        /// </summary>
        public ExchangeTradePair TradePair { get; set; }

        public ExchangeOrderType OrderType { get; set; }

        public decimal EstimatedCost { get; private set; }

        public decimal TransactionAmount { get; private set; }

        public Currency BaseCurrency { get; private set; }

        public decimal MaxExposureCost { get; private set; }

        public TimeSpan EstimatedTimeToExecute { get; private set; }

        public IExchangeTrader TradingIntegration { get; private set; }


        /// <summary>
        /// If we have a trade pair of for instance ETH - BTC then to exhange x-ETH to y-BTC we're doing a sell order.
        /// In other words the from currency is the one that dictates.
        /// </summary>
        public ExchangeOrderAction(IExchangeTrader tradingIntegration, ExchangeTradePair tradePair, ExchangeOrderType orderType, decimal transactionAmountInFromCurrency)
        {
            this.TradePair = tradePair;
            this.OrderType = orderType;
            this.BaseCurrency = this.TradePair.TradePair.ToCurrency; // The base currency is the to currency 
            this.EstimatedCost = this.TradePair.FeePercent / 100m * transactionAmountInFromCurrency;
            this.MaxExposureCost = 0m; // We set this to zero as order actions are instantaneous.
            this.EstimatedTimeToExecute = new TimeSpan(0, 0, 10); // We arbitrarly set the estimated time to execute to 10seconds. Generally, buying or selling at market price will be executed immediately.
            this.TradingIntegration = tradingIntegration;
            this.TransactionAmount = transactionAmountInFromCurrency;
        }

        public async Task ExecuteAction(bool simulate)
        {
            var fromAccount = this.TradePair.Exchange.TradeAccounts.Where(x => x.Currency == this.TradePair.TradePair.FromCurrency).Single();
            var toAccount = this.TradePair.Exchange.TradeAccounts.Where(x => x.Currency == this.TradePair.TradePair.ToCurrency).Single();
            
            // When the action is executed we expect the 
            //this.TradingIntegration.PlaceOrder()
            throw new NotImplementedException();
        }
    }
}
