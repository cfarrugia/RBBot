using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RBBot.Core.Models;

namespace RBBot.Core.Database
{
    public partial class RBBotContext : DbContext
    {
        public virtual DbSet<Currency> Currency { get; set; }
        public virtual DbSet<Exchange> Exchange { get; set; }
        public virtual DbSet<ExchangeSetting> ExchangeSetting { get; set; }
        public virtual DbSet<ExchangeStatus> ExchangeStatus { get; set; }
        public virtual DbSet<ExchangeTradePair> ExchangeTradePair { get; set; }
        public virtual DbSet<ExchangeTradePairStatus> ExchangeTradePairStatus { get; set; }
        public virtual DbSet<MarketPrice> MarketPrice { get; set; }
        public virtual DbSet<TradePair> TradePair { get; set; }


        public static string ConnectionString { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Currency>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasColumnType("varchar(5)");

                entity.Property(e => e.IsCrypto).HasDefaultValueSql("1");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Exchange>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);


                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Exchange)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Exchange_ExchangeStatus");


                entity.HasMany(x => x.ExchangeTradePair)
                .WithOne(y => y.Exchange)
                .HasForeignKey(y => y.ExchangeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ExchangeTradePair_Exchange");



            });

            modelBuilder.Entity<ExchangeSetting>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(1024);

                entity.HasOne(d => d.Exchange)
                    .WithMany(p => p.ExchangeSetting)
                    .HasForeignKey(d => d.ExchangeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ExchangeSetting_Exchange");
            });

            modelBuilder.Entity<ExchangeStatus>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.InverseIdNavigation)
                    .HasForeignKey<ExchangeStatus>(d => d.Id)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ExchangeStatus_ExchangeStatus");
            });

            modelBuilder.Entity<ExchangeTradePair>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.FeePercent).HasColumnType("decimal");

                entity.HasOne(d => d.Exchange)
                    .WithMany(p => p.ExchangeTradePair)
                    .HasForeignKey(d => d.ExchangeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ExchangeTradePair_Exchange");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.InverseIdNavigation)
                    .HasForeignKey<ExchangeTradePair>(d => d.Id)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ExchangeTradePair_ExchangeTradePair");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.ExchangeTradePair)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ExchangeTradePair_ExchangeTradePairStatus");

                entity.HasOne(d => d.TradePair)
                    .WithMany(p => p.ExchangeTradePair)
                    .HasForeignKey(d => d.TradePairId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ExchangeTradePair_TradePair");
            });

            modelBuilder.Entity<ExchangeTradePairStatus>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasColumnType("varchar(20)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");
            });

            modelBuilder.Entity<MarketPrice>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Price).HasColumnType("decimal");

                entity.Property(e => e.Timestamp).HasColumnType("datetime");

                entity.HasOne(d => d.ExchangeTradePair)
                    .WithMany(p => p.MarketPrice)
                    .HasForeignKey(d => d.ExchangeTradePairId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MarketPrice_ExchangeTradePair");
            });

            modelBuilder.Entity<TradePair>(entity =>
            {
                entity.HasOne(d => d.FromCurrency)
                    .WithMany(p => p.TradePairFromCurrency)
                    .HasForeignKey(d => d.FromCurrencyId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_TradePair_FromCurrency");

                entity.HasOne(d => d.ToCurrency)
                    .WithMany(p => p.TradePairToCurrency)
                    .HasForeignKey(d => d.ToCurrencyId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_TradePair_ToCurrency");
            });
        }
    }
}