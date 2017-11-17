using RBBot.Core.Database;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RBBot.Core.Engine.Trading
{
    /// <summary>
    /// This class defines a trading opportunity as proposed by the different price observers.
    /// An opportunity is a list of actions to be done immediately and others that 
    /// </summary>
    public abstract class Opportunity
    {
        /// <summary>
        /// This is the current value as a percentage of the base trade currency of this opportunity.
        /// This amount already excludes any fees involved in doing the arb. 
        /// </summary>
        public abstract decimal GetMarginValuePercent();

        /// <summary>
        /// This is the base currency of the opportunity. Use this with the margin to calculate actual monitary value.
        /// </summary>
        public Currency OpportunityBaseCurrency { get; set; }


        public abstract string TypeCode { get; }

        public abstract string UniqueIdentifier { get; }

        private SemaphoreSlim ExecutionSemaphore { get; set; } = new SemaphoreSlim(1); // Only one thread can do an execution at one point in time!

        /// <summary>
        /// This method should get the requirements needed for executing this ARB and check if the requirements are met.
        /// Persistence to DB its not the job of this method.
        /// </summary>
        /// <returns></returns>
        public abstract TradeOpportunityRequirement[] GetAndCheckRequirements();

        /// <summary>
        /// Gets the maximum amount that could be transacted. This is linked to the amount available in the 
        /// different accounts.
        /// </summary>
        public abstract decimal GetMaximumAmountThatCanBeTransacted();

        public bool RequirementsMet { get; protected set; }

        public decimal MaximumPossibleTransactionAmount { get; protected set; }

        /// <summary>
        /// This method will be used to return the root trade action to be executed for an opportunity. 
        /// </summary>
        public abstract ITradeAction GetTradeAction(decimal amount);

        public override string ToString()
        {
            return this.UniqueIdentifier;
        }

        public TradeOpportunityRequirement[] UpdateRequirementsAndAmount()
        {
            var requirements = GetAndCheckRequirements();
            this.MaximumPossibleTransactionAmount = this.GetMaximumAmountThatCanBeTransacted();
            this.RequirementsMet = requirements.All(x => x.RequirementMet);
            return requirements;
        }


        public decimal EstimateMaxUSDValue()
        {
            // The transaction amount is the max possible.
            var txAmount = this.GetMaximumAmountThatCanBeTransacted();

            // Calculate profit.
            var profit = this.MaximumPossibleTransactionAmount * this.GetMarginValuePercent()  / 100m * this.OpportunityBaseCurrency.ApproximateUSDValue;

            return profit;
        }


        /// <summary>
        /// Gets list of affected accounts in the opportunity object.
        /// </summary>
        public TradeAccount[] GetAffectedAccounts()
        {
            List<TradeAccount> accounts = new List<TradeAccount>();

            Action<ITradeAction> executeNode = null;
            executeNode = (node) =>
            {
                if (node.AffectedAccounts != null) accounts.AddRange(node.AffectedAccounts);
                if (node.ChildrenActions != null)
                    foreach (var child in node.ChildrenActions)
                        executeNode(child);
            };

            executeNode(this.GetTradeAction(0));

            return accounts.Distinct().ToArray();
        }

        /// <summary>
        /// This method is used to actually execute the opportunity
        /// </summary>
        /// <param name="simulate"></param>
        public async Task<TradeActionResponse> ExecuteOpportunity(decimal transactionAmount, bool simulate = true)
        {
            // Wait for semaphore.
            await this.ExecutionSemaphore.WaitAsync();

            try
            {
                var checksPassed = this.GetAndCheckRequirements().All(x => x.RequirementMet);
                if (!checksPassed) return new TradeActionResponse() { ExecutionSuccessful = false };

                List<TradeOpportunityTransaction> transactions = new List<TradeOpportunityTransaction>();
                List<TradeAccount> accounts = new List<TradeAccount>();

                // This anonymous function executes the action and waits for all children. 

                Func<ITradeAction, Task> executeNode = null;
                executeNode = async (action) =>
                {
                    // If to be executed before children...
                    if (action.ExecuteBeforeChildren)
                    {
                        var resp = Task.Run(() => action.ExecuteAction(simulate)).Result;
                        if ((resp != null) && resp.ExecutionSuccessful)
                        {
                            if (resp.Transactions != null) transactions.AddRange(resp.Transactions);
                        }
                    }


                    // Loop through the children.
                    if (action.ChildrenActions != null)
                    {
                        foreach (var child in action.ChildrenActions)
                        {
                            // If children to be executed asynchronously...
                            if (action.ExecuteChildrenInParallel)
                                await executeNode(child);
                            else
                                Task.Run(() => executeNode).Wait();
                        }
                    }

                    // If to be executed afterchildren...
                    if (!action.ExecuteBeforeChildren)
                    {
                        var resp = Task.Run(() => action.ExecuteAction(simulate)).Result;
                        if ((resp != null) && resp.ExecutionSuccessful)
                        {
                            if (resp.Transactions != null) transactions.AddRange(resp.Transactions);
                        }
                    }

                };

                var executionOk = checksPassed;
                try
                {
                    await executeNode(this.GetTradeAction(transactionAmount));
                }
                catch (Exception ex)
                {
                    // 
                    executionOk = false;
#warning make sure you capture the exception properly.
                }

                //
                return new TradeActionResponse()
                {
                    ExecutionSuccessful = executionOk,
                    Transactions = transactions.ToArray()
                };
            }
            finally
            {
                this.ExecutionSemaphore.Release(); // Release semaphore.
            }


            
        }

        #region Opportunity Helpers


        protected static TradeOpportunityRequirement GetExchangeTradeRequirement(Exchange exchange)
        {
            return new TradeOpportunityRequirement()
            {
                RequirementMet = (exchange.TradingIntegration) != null,
                ItemIdentifier = $"EXCHANGE - {exchange}",
                Message = (exchange.TradingIntegration) == null ? "The trading part of the exchange is not implemented" : "",
                Timestamp = DateTime.UtcNow,
                TradeOpportunityRequirementTypeId = TradeOpportunityRequirementType.RequirementTypes.Where(x => x.Code == "EXC-TRD-INT").Single().Id,
            };
        }

        protected static List<TradeOpportunityRequirement> GetAccountRequirements(string requirementPrefix, ExchangeTradePair tradePair, Currency currency, out TradeAccount account)
        {

            account = tradePair.Exchange.TradeAccounts.Where(x => x.Currency == currency).SingleOrDefault();

            var reqs = new List<TradeOpportunityRequirement>();

            reqs.Add(new TradeOpportunityRequirement()
            {
                RequirementMet = account != null,
                ItemIdentifier = $"{requirementPrefix} Account - {currency.Code}",
                Message = account == null ? $"Account in {currency.Code} doesn't exists on {tradePair.Exchange}" : "",
                Timestamp = DateTime.UtcNow,
                TradeOpportunityRequirementTypeId = TradeOpportunityRequirementType.RequirementTypes.Where(x => x.Code == "EXC-ACC-EXIST").Single().Id,
            });

            // if the first requirement is met, then proceed to the second one.
            if (reqs.First().RequirementMet)
            {
                reqs.Add(new TradeOpportunityRequirement()
                {
                    RequirementMet = account.Balance > 0m,
                    ItemIdentifier = $"{requirementPrefix} Account - {currency.Code}",
                    Message = account.Balance <= 0m ? $"Account in {currency.Code} has zero balance on {tradePair.Exchange}" : "",
                    Timestamp = DateTime.UtcNow,
                    TradeOpportunityRequirementTypeId = TradeOpportunityRequirementType.RequirementTypes.Where(x => x.Code == "EXC-ACC-BAL").Single().Id,
                });

                bool noAddress = string.IsNullOrEmpty(account.Address);

                reqs.Add(new TradeOpportunityRequirement()
                {
                    RequirementMet = !noAddress,
                    ItemIdentifier = $"{requirementPrefix} Account - {currency.Code}",
                    Message = noAddress ? $"Account in {currency.Code} has no address on {tradePair.Exchange}" : "",
                    Timestamp = DateTime.UtcNow,
                    TradeOpportunityRequirementTypeId = TradeOpportunityRequirementType.RequirementTypes.Where(x => x.Code == "EXC-ACC-ADDR").Single().Id,
                });
            }

            return reqs;
        }

        #endregion

    }
}
