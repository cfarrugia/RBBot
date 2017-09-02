namespace RBBot.Core.Database
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using RBBot.Core.Models;

    public partial class RBBotContext : DbContext
    {
        public RBBotContext()
            : base("name=RBBot")
        {
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }
        

        public virtual DbSet<Currency> Currencies { get; set; }
        public virtual DbSet<Exchange> Exchanges { get; set; }
        public virtual DbSet<ExchangeSetting> ExchangeSettings { get; set; }
        public virtual DbSet<ExchangeState> ExchangeStates { get; set; }
        public virtual DbSet<ExchangeTradePair> ExchangeTradePairs { get; set; }
        public virtual DbSet<ExchangeTradePairState> ExchangeTradePairStates { get; set; }
        public virtual DbSet<MarketPrice> MarketPrices { get; set; }
        public virtual DbSet<TradeAccount> TradeAccounts { get; set; }
        public virtual DbSet<TradeOpportunity> TradeOpportunities { get; set; }
        public virtual DbSet<TradeOpportunityValue> TradeOpportunityValues { get; set; }
        public virtual DbSet<TradeOpportunityType> TradeOpportunityTypes { get; set; }
        public virtual DbSet<TradePair> TradePairs { get; set; }
        public virtual DbSet<TradeOpportunityTransaction> TradeOpportunityTransactions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Currency>()
                .Property(e => e.Code)
                .IsUnicode(false);

            modelBuilder.Entity<Exchange>()
                .HasMany(e => e.ExchangeSettings)
                .WithRequired(e => e.Exchange)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Exchange>()
                .HasMany(e => e.ExchangeTradePairs)
                .WithRequired(e => e.Exchange)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Exchange>()
                .HasMany(e => e.TradeAccounts)
                .WithRequired(e => e.Exchange)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Exchange>()
                .HasMany(e => e.TradeOpportunityTransactions)
                .WithRequired(e => e.Exchange)
                .HasForeignKey(e => e.ExecuteOnExchangeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ExchangeSetting>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<ExchangeState>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<ExchangeState>()
                .Property(e => e.Code)
                .IsUnicode(false);

            modelBuilder.Entity<ExchangeState>()
                .HasMany(e => e.Exchanges)
                .WithRequired(e => e.ExchangeState)
                .HasForeignKey(e => e.StateId)
                .WillCascadeOnDelete(false);


            modelBuilder.Entity<ExchangeTradePair>()
                .Property(e => e.FeePercent)
                .HasPrecision(19, 4);

            modelBuilder.Entity<ExchangeTradePairState>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<ExchangeTradePairState>()
                .Property(e => e.Code)
                .IsUnicode(false);

            modelBuilder.Entity<ExchangeTradePairState>()
                .HasMany(e => e.ExchangeTradePairs)
                .WithRequired(e => e.ExchangeTradePairState)
                .HasForeignKey(e => e.StateId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MarketPrice>()
                .Property(e => e.Price)
                .HasPrecision(18, 8);

            modelBuilder.Entity<TradeAccount>()
                .Property(e => e.Balance)
                .HasPrecision(18, 8);

            modelBuilder.Entity<TradeOpportunity>()
                .Property(e => e.StartTime)
                .IsRequired();

            modelBuilder.Entity<TradeOpportunity>()
                .Property(e => e.ExecutedTime)
                .IsOptional();

            modelBuilder.Entity<TradeOpportunity>()
                .Property(e => e.EndTime)
                .IsOptional();


            modelBuilder.Entity<TradeOpportunity>()
                .HasMany(e => e.TradeOpportunityTransactions)
                .WithRequired(e => e.TradeOpportunity)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TradeOpportunity>()
                .HasMany(e => e.TradeOpportunityTransactions)
                .WithRequired(e => e.TradeOpportunity)
                .WillCascadeOnDelete(false);

            //modelBuilder.Entity<TradeOpportunity>()
            //    .HasMany(e => e.TradeOpportunityValues)
            //    .WithRequired(e => e.TradeOpportunity)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<TradeOpportunityType>()
                .Property(e => e.Code)
                .IsUnicode(false);

            modelBuilder.Entity<TradeOpportunityType>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<TradePair>()
                .HasMany(e => e.ExchangeTradePairs)
                .WithRequired(e => e.TradePair)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TradeOpportunityTransaction>()
                .Property(e => e.FromAmount)
                .HasPrecision(18, 8);

            modelBuilder.Entity<TradeOpportunityTransaction>()
                .Property(e => e.ToAmount)
                .HasPrecision(18, 8);

            modelBuilder.Entity<TradeOpportunityTransaction>()
                .Property(e => e.FromAccountFee)
                .HasPrecision(18, 8);

            modelBuilder.Entity<TradeOpportunityTransaction>()
                .Property(e => e.ToAccountFee)
                .HasPrecision(18, 8);

            modelBuilder.Entity<TradeOpportunityTransaction>()
                .Property(e => e.FromAccountBalanceBeforeTx)
                .HasPrecision(18, 8);

            modelBuilder.Entity<TradeOpportunityTransaction>()
                .Property(e => e.ToAccountBalanceBeforeTx)
                .HasPrecision(18, 8);

            modelBuilder.Entity<TradeOpportunityTransaction>()
                .Property(e => e.ExchangeRate)
                .HasPrecision(18, 0);

            modelBuilder.Entity<TradeOpportunityTransaction>()
                .Property(e => e.EstimatedFromAccountBalanceAfterTx)
                .HasPrecision(18, 8);

            modelBuilder.Entity<TradeOpportunityTransaction>()
                .Property(e => e.EstimatedToAccountBalanceAfterTx)
                .HasPrecision(18, 8);
        }
    }
}
