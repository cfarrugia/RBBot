using RBBot.Core.Database;
using RBBot.Core.Engine.Trading;
using RBBot.Core.Helpers;
using RBBot.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RBBot.Core.Engine
{
    public static class OpportunityScoreEngine
    {
        private static ConcurrentDictionary<string, TradeOpportunity> tradeOpportunities;


        // Retains an observable over the expired opportunities.
        private static IObservable<TradeOpportunity> expiredOpportunities = null;

        /// <summary>
        /// Event generates when an opportunity expires.
        /// </summary>
        private static event Action<TradeOpportunity> OnOpportunityExpired;

        public static void InitializeEngine()
        {
            // Initialize the dictionary.
            tradeOpportunities = new ConcurrentDictionary<string, TradeOpportunity>();

            // And set out a task to monitor the values and clean old opportunities.
            Observable.Interval(new TimeSpan(0, 0, SystemSetting.OpportunityTimeoutInSeconds)).Subscribe(x =>
              {
                  foreach (var kp in tradeOpportunities.Keys.ToList())
                  {
                      TradeOpportunity tradeOpp = null;
                      if ((tradeOpportunities.TryGetValue(kp, out tradeOpp)) && (tradeOpp.LastestUpdate.AddSeconds(SystemSetting.OpportunityTimeoutInSeconds) < DateTime.UtcNow))
                      {
                          // End the opportunity with expiration as the reason.
                          Task.Run(() => EndTradeOpportunity(tradeOpp, TradeOpportunityState.States.Single(z => z.Code == "EXP-TIMEOUT"))); 
                      }
                  }
              });

            // Create an observable hooked up whenever an opportunity expiration happens. This is going to be useful later.
            expiredOpportunities = Observable.FromEvent<TradeOpportunity>(x => OnOpportunityExpired += x, y => OnOpportunityExpired -= y);
        }

        public static IObservable<TradeOpportunity> GetTradeOpportunityStream(this IObservable<Opportunity> opportunity, bool IsSimulation = true)
        {
            // Convert opportunities to TradeOpportunities.
            var tradeStream =
                from o in opportunity
                from oo in Observable.FromAsync<TradeOpportunity>(() => GetOrUpdateTradeOpportunity(o, IsSimulation))
                select oo;

            // Merge the stream of expired tradeopportunities too.
            // Remove any null opportunities. 
            tradeStream = tradeStream
                .Merge(expiredOpportunities).Where(x => x != null)
                .Catch<TradeOpportunity, Exception>(err =>
                 {
                     Console.WriteLine($"Error occurred in trade stream: {err.ToString()}");
                     return Observable.Empty<TradeOpportunity>();
                 }); // And absorb any errors!

            return tradeStream;
        }

        public static async Task ExecuteTradeOpportunity(TradeOpportunity opportunity, bool isSimulation = true)
        {
            // if the opportunity is being executed, return now.
            if (opportunity.IsExecuting || opportunity.IsExecuted) return;

            try
            {
                
                opportunity.IsExecuting = true;
                var result = await opportunity.LatestOpportunity.ExecuteOpportunity(opportunity.LatestOpportunity.MaximumPossibleTransactionAmount, true);

               
                // If execution was ok, then add the transactions. Otherwise signal a failed execution.
                var finalStateCode = isSimulation ? "EXEC-SIM" : "EXEC-REAL";
                if (!result.ExecutionSuccessful)
                {
                    finalStateCode = finalStateCode + "-ERR";
                    result = null; // Make result none, so we don't affect accounts and transactions!
                }

                var finalState = TradeOpportunityState.States.Single(x => x.Code == finalStateCode);

                opportunity.IsExecuted = true;

                await OpportunityScoreEngine.EndTradeOpportunity(opportunity, finalState, null, result);
            }
            catch (Exception ex)
            {


                Console.WriteLine();
            }

        }

        public static async Task EndTradeOpportunity(TradeOpportunity opportunity, TradeOpportunityState finalState, TradeOpportunityValue finalOpportunityValue = null, TradeActionResponse tradeActionResponse = null)
        {
            // Try removing the opportunity from the dictionary.
            TradeOpportunity time = null;
            bool removedSuccess = tradeOpportunities.TryRemove(opportunity.LatestOpportunity.UniqueIdentifier, out time);

            if (removedSuccess == false) // If the opportunity couldn't be removed, then return. 
                return;

            opportunity.TradeOpportunityStateId = finalState.Id;
            opportunity.EndTime = DateTime.UtcNow;

            // Call expired event.
            if (OnOpportunityExpired != null) OnOpportunityExpired(opportunity);



            // This is the part we persist to the db. We first wait for 
            // any possible semaphore on the opportunity and release it as soon as we're done. 

            await opportunity.LockingSemaphore.WaitAsync();
            
            try
            {
                // Persist to db
                using (var ctx = new RBBotContext())
                {
                    // If a final value is specified, add it now.
                    if (finalOpportunityValue != null)
                    {
                        finalOpportunityValue.TradeOpportunityId = opportunity.Id;
                        ctx.TradeOpportunityValues.Add(finalOpportunityValue);
                    }

                    // With transactions we also look into the accounts and update them.
                    if (tradeActionResponse != null)
                    {
                        tradeActionResponse.Transactions.ToList().ForEach(x => x.TradeOpportunity = opportunity);
                        ctx.TradeOpportunityTransactions.AddRange(tradeActionResponse.Transactions);

                        foreach (var acc in tradeActionResponse.AffectedAccounts)
                        {
                            ctx.TradeAccounts.Attach(acc);
                            ctx.Entry(acc).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    ctx.TradeOpportunities.Attach(opportunity);
                    ctx.Entry(opportunity).State = System.Data.Entity.EntityState.Modified;

                    await ctx.SaveChangesAsync();
                }
            }
            finally
            {
                opportunity.LockingSemaphore.Release();
            }
            
        }

        private static async Task<TradeOpportunity> GetOrUpdateTradeOpportunity(Opportunity opportunity, bool IsSimulation)
        {
            // Update requirements and maximum possible trade.
            var requirements = opportunity.UpdateRequirementsAndAmount().ToList();

            //
            string stateCode = opportunity.RequirementsMet ? "RQ-MET" : "RQ-MISSING";

            decimal oppValue = opportunity.GetValue();

            // Create the trade value;
            var newTradeValue = new TradeOpportunityValue()
            {
                PotentialMargin = oppValue,
                Timestamp = DateTime.UtcNow,
                TradeOpportunityStateId = TradeOpportunityState.States.Single(x => x.Code == stateCode).Id
            };

            // If the oppvalue is below treshold and doesn't exist yet, then ignore it. Else stop it.
            if (oppValue < SystemSetting.MinimumTradeOpportunityPercent)
            {

                TradeOpportunity tradOpp = null;
                tradeOpportunities.TryGetValue(opportunity.UniqueIdentifier, out tradOpp);

                if (tradOpp == null)
                {
                    return null;
                }
                else
                {
                    var belowThresholdState = TradeOpportunityState.States.Single(x => x.Code == "EXP-BELOW");
                    newTradeValue.TradeOpportunityStateId = belowThresholdState.Id;
                    await EndTradeOpportunity(tradOpp, belowThresholdState, newTradeValue);
                    return null;
                }
            }


            // Try getting the TradeOpp from the dictionary. If it doesn't exist, then construct it now
            var isNewOpportunity = false;
            var tradeOpp = tradeOpportunities.GetOrAdd(opportunity.UniqueIdentifier, (key) =>
            {
                isNewOpportunity = true;
                return new TradeOpportunity()
                {
                    CurrencyId = opportunity.OpportunityBaseCurrency.Id,
                    IsExecuted = false,
                    IsSimulation = IsSimulation,
                    StartTime = DateTime.UtcNow,
                    TradeOpportunityTypeId = TradeOpportunityType.Types.Single(x => x.Code == opportunity.TypeCode).Id,
                    TradeOpportunityStateId = TradeOpportunityState.States.Single(x => x.Code == stateCode).Id,
                    Description = opportunity.UniqueIdentifier,
                    LatestOpportunity = opportunity,
                    LastestUpdate = DateTime.UtcNow
                };
            }
            );

            tradeOpp.LatestOpportunity = opportunity;
            tradeOpp.LastestUpdate = DateTime.UtcNow;


            // Before writing, we lock the semaphore.
            await tradeOpp.LockingSemaphore.WaitAsync(); // Lock 

            try
            {
                // Otherwise the opportunity is still valid. 
                // If this is a new opportunity, then we definitely need to save to DB.
                if (isNewOpportunity)
                {


                    using (var ctx = new RBBotContext())
                    {

                        requirements.ForEach(x => { x.TradeOpportunity = tradeOpp; tradeOpp.TradeOpportunityRequirements.Add(x); });
                        tradeOpp.TradeOpportunityValues.Add(newTradeValue);
                        newTradeValue.TradeOpportunity = tradeOpp;

                        ctx.TradeOpportunities.Add(tradeOpp);
                        ctx.TradeOpportunityRequirements.AddRange(requirements);
                        ctx.TradeOpportunityValues.Add(newTradeValue);


                        await ctx.SaveChangesAsync();
                    }

                }
                else
                {
                    // Else we take the requirements and see if anything changed.
                    var requirementsToAdd = new List<TradeOpportunityRequirement>();

                    foreach (var req in requirements)
                    {
                        var toReq = tradeOpp.TradeOpportunityRequirements.Where(x => x.ItemIdentifier == req.ItemIdentifier && x.TradeOpportunityRequirementType == req.TradeOpportunityRequirementType).OrderBy(x => x.Timestamp).LastOrDefault();

                        if (toReq == null || toReq.RequirementMet != req.RequirementMet)
                        {
                            requirementsToAdd.Add(req);
                            req.TradeOpportunityId = tradeOpp.Id;
                        }
                    }

                    newTradeValue.TradeOpportunityId = tradeOpp.Id;

                    using (var ctx = new RBBotContext())
                    {
                        ctx.TradeOpportunityRequirements.AddRange(requirementsToAdd);
                        ctx.TradeOpportunityValues.Add(newTradeValue);
                        await ctx.SaveChangesAsync();
                    }
                }

            }
            finally
            {
                tradeOpp.LockingSemaphore.Release();
            }

            
            // Return the trade opportunity object. 
            return tradeOpp;
        }
    }
}
