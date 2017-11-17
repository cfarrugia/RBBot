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

        public TradeAccount[] AffectedAccounts { get { return new TradeAccount[] { this.FromAccount, this.ToAccount }; } }


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

        public TradeAccount FromAccount { get; private set; }
        public TradeAccount ToAccount { get; private set; }


        /// <summary>
        /// If we have a trade pair of for instance ETH - BTC then to exhange x-ETH to y-BTC we're doing a sell order.
        /// In other words the from currency is the one that dictates.
        /// </summary>
        public ExchangeOrderAction(ExchangeTradePair tradePair, ExchangeOrderType orderType, decimal txAmountInPreferredCurrency, Currency txCurrency)
        {
            this.TradePair = tradePair;
            this.OrderType = orderType;
            this.BaseCurrency = txCurrency; // The base currency is the to currency 
            this.EstimatedCost = this.TradePair.FeePercent / 100m * txAmountInPreferredCurrency;
            this.MaxExposureCost = 0m; // We set this to zero as order actions are instantaneous.
            this.EstimatedTimeToExecute = new TimeSpan(0, 0, 10); // We arbitrarly set the estimated time to execute to 10seconds. Generally, buying or selling at market price will be executed immediately.
            this.TradingIntegration = tradePair.Exchange.TradingIntegration; // It's good to assume that if this point is reached then the exchange trading was integrated.
            this.TransactionAmount = txAmountInPreferredCurrency;

            // In a trade pair of ETH -> BTC we always consider the from as the ETH
            this.FromAccount = this.TradePair.Exchange.TradeAccounts.Where(x => x.Currency == this.TradePair.TradePair.FromCurrency).Single();
            this.ToAccount = this.TradePair.Exchange.TradeAccounts.Where(x => x.Currency == this.TradePair.TradePair.ToCurrency).Single();

        }

        public async Task<TradeActionResponse> ExecuteAction(bool simulate)
        {

            // Either from or to account must be the base currency. Else throw exception.
            if (this.FromAccount.Currency != this.BaseCurrency && this.ToAccount.Currency != this.BaseCurrency)
                throw new NotSupportedException($"It is not supported to have trade actions where {this.BaseCurrency.Code} is not involved");

            var resp = new TradeActionResponse();

            // This is where money is spent... be cautious
            ExchangeOrderResponse orderResponse = null;
            if (simulate == false)
            {
                orderResponse = await TradingIntegration.PlaceOrder(this.OrderType, this.TransactionAmount, this.BaseCurrency, this.TradePair);
            }
            else
            {
                orderResponse = new ExchangeOrderResponse() { Success = true, ExternalTransactionId = "Simulation-Tx" };
                orderResponse.Fee = TradingIntegration.EstimateTransactionFee(this.OrderType, this.TransactionAmount, this.BaseCurrency, this.TradePair);
            }


            if (orderResponse.Success == false)
            {
                resp.ExecutionSuccessful = false;
                return resp;
            }; // Return nothing if there was a problem.


            // If the order type is a sell, then the FromAccount decreases and the ToAccount balance increases.
            // This means that if we're selling, then we'll have less ETH (from amount) and more BTC (to amount)
            // Therefore the from amount difference is just the TransactionAmount and +ve/-ve according to whether its a buy or sell.
            var fromAmount = this.TransactionAmount * (this.OrderType == ExchangeOrderType.Buy ? 1 : -1);
            var toAmount = -1 * fromAmount;

            //
            var exchangeRate = this.TradePair.LatestPrice;

            // Now we do the currency conversion. We'll do the conversion on the non-preferred currency.
            if (this.FromAccount.Currency == this.BaseCurrency)
                toAmount = toAmount / exchangeRate;
            else
                fromAmount = fromAmount / exchangeRate;


            var fromAccountFee = orderResponse.Fee.Currency == this.TradePair.TradePair.FromCurrency ? orderResponse.Fee.Amount : 0m;
            var toAccountFee = orderResponse.Fee.Currency == this.TradePair.TradePair.ToCurrency ? orderResponse.Fee.Amount : 0m;

            var fromAccountBalanceBefore = FromAccount.Balance;
            var toAccountBalanceBefore = ToAccount.Balance;
            var fromAccountBalanceAfter = fromAccountBalanceBefore + fromAmount - fromAccountFee;
            var toAccountBalanceAfter =  toAccountBalanceBefore + toAmount - toAccountFee;

            var tx = new TradeOpportunityTransaction()
            {
                CreationDate = DateTime.UtcNow,
                ExchangeRate = exchangeRate,
                FromAccountId = FromAccount.Id,
                ToAccountId = ToAccount.Id,
                ExternalTransactionId = orderResponse.ExternalTransactionId,
                ExecutedOnExchangeId = this.TradePair.Exchange.Id,
                IsReal = !simulate,
                FromAccountFee = fromAccountFee,
                ToAccountFee = toAccountFee,
                FromAmount = fromAmount,
                ToAmount = toAmount,
                FromAccountBalanceBeforeTx = fromAccountBalanceBefore,
                ToAccountBalanceBeforeTx = toAccountBalanceBefore,
                EstimatedFromAccountBalanceAfterTx = fromAccountBalanceAfter,
                EstimatedToAccountBalanceAfterTx = toAccountBalanceAfter,
            };

            FromAccount.Balance = fromAccountBalanceAfter;
            ToAccount.Balance = toAccountBalanceAfter;
            FromAccount.LastUpdate = DateTime.UtcNow;
            ToAccount.LastUpdate = DateTime.UtcNow;

            // Return the response.
            resp.Transactions = new TradeOpportunityTransaction[] { tx };
            resp.ExecutionSuccessful = true;

            return resp;
        }
    }
}
