using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RBBot.Core.Models;
using RBBot.Core.Exchanges;
using RBBot.Core.Database;

namespace RBBot.Core.Engine.Trading.Actions
{
    public class CurrencyTransfer : ITradeAction
    {
        public Currency BaseCurrency { get; private set; }

        public TradeAccount[] AffectedAccounts { get { return new TradeAccount[] { this.FromAccount, this.ToAccount }; } }

        public decimal EstimatedCost { get; private set; }

        public TimeSpan EstimatedTimeToExecute { get; private set; }

        public decimal MaxExposureCost { get; private set; }

        public TradeAccount FromAccount { get; private set; }
        public TradeAccount ToAccount { get; private set; }

        public IExchangeTrader FromTraderIntegration { get; private set; }
        public IExchangeTrader ToTraderIntegration { get; private set; }
        public decimal TransactionAmount { get; private set; }


        public ITradeAction[] ChildrenActions { get; set; }
        public bool ExecuteChildrenInParallel { get; set; }
        public bool ExecuteBeforeChildren { get; set; }

        public CurrencyTransfer(Currency currency, IExchangeTrader fromExchangeTrader, IExchangeTrader toExchangeTrader, decimal transferAmount)
        {
            this.FromTraderIntegration = fromExchangeTrader;
            this.ToTraderIntegration = toExchangeTrader;
            this.FromAccount = fromExchangeTrader.Exchange.TradeAccounts.Where(x => x.Currency == currency).Single();
            this.ToAccount = toExchangeTrader.Exchange.TradeAccounts.Where(x => x.Currency == currency).Single();
            this.EstimatedCost = currency.AverageTransferFee;
            this.EstimatedTimeToExecute = new TimeSpan(0, currency.AverageTransferTimeMinutes, 0);
            this.BaseCurrency = currency;
            this.TransactionAmount = TransactionAmount;
            this.MaxExposureCost = this.BaseCurrency.DailyVolatilityIndex / (24 * 60 * 100m) * transferAmount * currency.AverageTransferTimeMinutes; // At worse, this is the maximum amount one would expect to loose during the transfer of funds
        }

        public async Task<TradeActionResponse> ExecuteAction(bool simulate)
        {
            // Decrement from account, increment to account.
            this.FromAccount.Balance -= this.EstimatedCost + this.TransactionAmount;
            this.ToAccount.Balance += this.TransactionAmount;

            var resp = new TradeActionResponse()
            {
                ExecutionSuccessful = true,
                Transactions = null
            };

            using (var ctx = new RBBotContext())
            {
                ctx.Entry(this.FromAccount).State = System.Data.Entity.EntityState.Modified;
                ctx.Entry(this.ToAccount).State = System.Data.Entity.EntityState.Modified;
                await ctx.SaveChangesAsync();

                if (!simulate)
                {
#warning Still to implement the actual thing!!
                }

                throw new NotImplementedException();
            }

            return resp;
        }
    }
}

