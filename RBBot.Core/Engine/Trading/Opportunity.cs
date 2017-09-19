using RBBot.Core.Database;
using RBBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public decimal CurrentValue { get; private set; }



        /// <summary>
        /// This is the opportunity model object used in the db to track the opportunity changes.
        /// </summary>
        public TradeOpportunity OpportunityModel = null;

        /// <summary>
        /// This is the base currency of the opportunity. Use this with the margin to calculate actual monitary value.
        /// </summary>
        public Currency OpportunityBaseCurrency { get; set; }

        public abstract string TypeCode { get; }
        
        /// <summary>
        /// This method should get the requirements needed for executing this ARB and check if the requirements are met.
        /// Persistence to DB its not the job of this method.
        /// </summary>
        /// <returns></returns>
        public abstract Task<TradeOpportunityRequirement[]> GetAndCheckRequirements();


        /// <summary>
        /// Gets the maximum amount that could be transacted. This is linked to the amount available in the 
        /// different accounts.
        /// </summary>
        public abstract decimal GetMaximumAmountThatCanBeTransacted();

        /// <summary>
        /// This method will be used to return the root trade action to be executed for an opportunity. 
        /// </summary>
        public abstract ITradeAction GetTradeAction(decimal amount);
        
        private async Task<bool> GetCheckAndPersistRequirements(RBBotContext dbCtx)
        {
            var requirements = await this.GetAndCheckRequirements();

            var checksOk = !requirements.Any(x => x.RequirementMet == false);

            dbCtx.TradeOpportunityRequirements.AddRange(requirements);

            await dbCtx.SaveChangesAsync();

            return checksOk;
        }

        /// <summary>
        /// This method is used to actually execute the opportunity
        /// </summary>
        /// <param name="simulate"></param>
        public async Task<bool> ExecuteOpportunity(decimal transactionAmount, bool simulate = true)
        {
            // Before executing, we do one last requirements check. 
            using (var ctx = new RBBotContext())
            {
                var checksPassed = await this.GetCheckAndPersistRequirements(ctx);
                List<TradeOpportunityTransaction> transactions = new List<TradeOpportunityTransaction>();

                // This anonymous function executes the action and waits for all children. 
                Func<ITradeAction, Task> executeNode = null;
                executeNode = async (action) => 
                   {
                       

                       // If to be executed before children...
                       if (action.ExecuteBeforeChildren)
                       {
                           var tx = Task.Run(() => action.ExecuteAction(simulate)).Result;
                           if (tx != null) transactions.Add(tx);
                       }


                       // Loop through the children.
                       foreach (var child in action.ChildrenActions)
                       {
                           // If children to be executed asynchronously...
                           if (action.ExecuteChildrenInParallel)
                               await executeNode(child);
                           else
                               Task.Run(() => executeNode).Wait();
                       }
                       
                       // If to be executed afterchildren...
                       if (!action.ExecuteBeforeChildren)
                       {
                           var tx = Task.Run(() => action.ExecuteAction(simulate)).Result;
                           if (tx != null) transactions.Add(tx);
                       }

                   };


                var executionOk = checksPassed;

                // If the checks have passed, then execute all actions!
                if (checksPassed)
                {
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
                }

                // Save all the transaction actions to database.
                ctx.TradeOpportunityTransactions.AddRange(transactions);
                this.OpportunityModel.IsExecuted = true;
                this.OpportunityModel.EndTime = DateTime.UtcNow;
                await ctx.SaveChangesAsync();

                //
                return executionOk;
            }
        }


        internal async Task UpdateOpportunity(decimal newMarginValue)
        {
            this.CurrentValue = newMarginValue;

            // the opportunity is only considered started on first non-zero value!
            if (newMarginValue == 0m) return;

            using (var ctx = new RBBotContext())
            {
                if (this.OpportunityModel == null)
                {

                    this.OpportunityModel = new TradeOpportunity()
                    {
                        CurrencyId = this.OpportunityBaseCurrency.Id,
                        StartTime = DateTime.UtcNow,
                        IsExecuted = false,
                        TradeOpportunityTypeId = ctx.TradeOpportunityTypes.Single(x => x.Code == this.TypeCode).Id
                    };


                    lock (this.OpportunityModel)
                    {
                        // Add it to the context.
                        ctx.TradeOpportunities.Add(this.OpportunityModel);
                        ctx.SaveChanges(); // Don't execute this async so we avoid having the children created before the parent!
                    }
                }
               
                var newTradeValue = new TradeOpportunityValue()
                {
                    PotentialMargin = newMarginValue,
                    Timestamp = DateTime.UtcNow,
                    TradeOpportunityId = this.OpportunityModel.Id
                };

                
                ctx.TradeOpportunityValues.Add(newTradeValue);
                //this.OpportunityModel.TradeOpportunityValues.Add(newTradeValue);
                //ctx.Entry(this.OpportunityModel).State = System.Data.Entity.EntityState.Modified;
                //ctx.TradeOpportunityValues.Add(newTradeValue);

                try
                {
                    await ctx.SaveChangesAsync();
                }
                catch (Exception ex)
                {

                    Console.WriteLine("Error saving opportunity to database: ", ex.ToString());

                }
                

                // async save to db and return
                //await ctx.SaveChangesAsync();

                // 
                await OpportunityScoreEngine.UpdateOpportunityValue(this);
            }



        }

    }
}
