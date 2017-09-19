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

        public ITradeAction[] ChildrenActions { get; set; }

        public bool ExecuteChildrenInParallel { get; set; }

        public bool ExecuteBeforeChildren { get; set; }


        /// <summary>
        /// If we have a trade pair of for instance ETH - BTC then to exhange x-ETH to y-BTC we're doing a sell order.
        /// In other words the from currency is the one that dictates.
        /// </summary>
        public ExchangeOrderAction(ExchangeTradePair tradePair, ExchangeOrderType orderType, decimal transactionAmountInFromCurrency)
        {
            this.TradePair = tradePair;
            this.OrderType = orderType;
            this.BaseCurrency = this.TradePair.TradePair.ToCurrency; // The base currency is the to currency 
            this.EstimatedCost = this.TradePair.FeePercent / 100m * transactionAmountInFromCurrency;
            this.MaxExposureCost = 0m; // We set this to zero as order actions are instantaneous.
            this.EstimatedTimeToExecute = new TimeSpan(0, 0, 10); // We arbitrarly set the estimated time to execute to 10seconds. Generally, buying or selling at market price will be executed immediately.
            this.TradingIntegration = tradePair.Exchange.TradingIntegration; // It's good to assume that if this point is reached then the exchange trading was integrated.
            this.TransactionAmount = transactionAmountInFromCurrency;
        }

        public async Task<TradeOpportunityTransaction> ExecuteAction(bool simulate)
        {
           
            // This is where money is spent... be cautious
            ExchangeOrderResponse orderResponse = null;
            if (simulate == false)
            {
                orderResponse = await TradingIntegration.PlaceOrder(this.OrderType, this.TransactionAmount, this.TradePair);
            }
            else
            {
                orderResponse = new ExchangeOrderResponse() { Success = true, ExternalTransactionId = "Simulation-Tx" };
                orderResponse.Fee = TradingIntegration.EstimateTransactionFee(this.OrderType, this.TransactionAmount, this.TradePair);
            }


            if (orderResponse.Success == false) return null; // Return nothing if there was a problem.

            // In a trade pair of ETH -> BTC we always consider the from as the ETH
            var fromAccount = this.TradePair.Exchange.TradeAccounts.Where(x => x.Currency == this.TradePair.TradePair.FromCurrency).Single();
            var toAccount = this.TradePair.Exchange.TradeAccounts.Where(x => x.Currency == this.TradePair.TradePair.ToCurrency).Single();

            //
            var exchangeRate = this.TradePair.LatestPrice;

            // This means that if we're selling, then we'll have less ETH (from amount) and more BTC (to amount)
            // Therefore the from amount difference is just the TransactionAmount and +ve/-ve according to whether its a buy or sell.
            var fromAmount = this.TransactionAmount * (this.OrderType == ExchangeOrderType.Buy ? 1 : -1);

            // The to amount difference needs to take the exchange rate in consideration and has the inverse value of the from account.
            var toAmount = -1 * exchangeRate * fromAmount;



            // If the order type is a sell, then the FromAccount decreases and the ToAccount balance increases.
            var fromAccountFee = orderResponse.Fee.Currency == this.TradePair.TradePair.FromCurrency ? orderResponse.Fee.Amount : 0m;
            var toAccountFee = orderResponse.Fee.Currency == this.TradePair.TradePair.ToCurrency ? orderResponse.Fee.Amount : 0m;

            var fromAccountBalanceBefore = fromAccount.Balance;
            var toAccountBalanceBefore = toAccount.Balance;
            var fromAccountBalanceAfter = fromAccountBalanceBefore + fromAmount + fromAccountFee;
            var toAccountBalanceAfter =  toAccountBalanceBefore + toAmount + toAccountFee;

            var tx = new TradeOpportunityTransaction()
            {
                CreationDate = DateTime.UtcNow,
                ExchangeRate = exchangeRate,
                FromAccount = fromAccount,
                ToAccount = toAccount,
                ExternalTransactionId = orderResponse.ExternalTransactionId,
                ExecutedOnExchange = this.TradePair.Exchange,
                IsReal = !simulate,
                TradeOpportunity = null, // not set here
                FromAccountFee = fromAccountFee,
                ToAccountFee = toAccountFee,
                FromAmount = fromAmount,
                ToAmount = toAmount,
                FromAccountBalanceBeforeTx = fromAccountBalanceBefore,
                ToAccountBalanceBeforeTx = toAccountBalanceBefore,
                EstimatedFromAccountBalanceAfterTx = fromAccountBalanceAfter,
                EstimatedToAccountBalanceAfterTx = toAccountBalanceAfter
            };

            // Return the transaction object.
            return tx;
        }
    }
}
