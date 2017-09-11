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
        /// This property stores the actions to be taken to execute this opportunity.
        /// Note that this is a jagged array. Anything in the first index is executed in sync. anything in the second array is executed async.
        /// </summary>
        public ITradeAction[][] Actions { get; protected set; }

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
        /// This method is used to actually execute the opportunity
        /// </summary>
        /// <param name="simulate"></param>
        public async Task ExecuteOpportunity(bool simulate = true)
        {
            // Loop through all actions.
            foreach (var syncActions in this.Actions)
            {
                // Wait all tasks to finish, but launch in parallel!
                Task.WaitAll(syncActions.Select(x => x.ExecuteAction(simulate)).ToArray());
            }

            using (var ctx = new RBBotContext())
            {
                if (this.OpportunityModel != null) return; // Should never be the case, but never know.

#warning this is incomplete!!
                lock (this.OpportunityModel)
                {


                }
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
