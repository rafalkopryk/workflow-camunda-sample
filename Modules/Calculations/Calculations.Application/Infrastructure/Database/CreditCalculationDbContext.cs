using Calculations.Application.Domain;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Calculations.Application.Infrastructure.Database;

public class CreditCalculationDbContext : DbContext
{
    public DbSet<CreditCalculation> Calculations { get; set; }

    public CreditCalculationDbContext(DbContextOptions<CreditCalculationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (Database.IsCosmos())
        {
            modelBuilder.HasManualThroughput(400);
        }

        modelBuilder.Entity<CreditCalculation>(entity =>
        {
            if (Database.IsCosmos())
            {
                entity.ToContainer("Calculations");
                entity.HasNoDiscriminator();
            }

            if (Database.IsSqlServer())
            {
                entity.ToTable("CreditCalculation");
            }

            if (Database.IsNpgsql())
            {
                entity.ToTable("CreditApplication");
            }
            
            if (Database.ProviderName == "MongoDB.EntityFrameworkCore")
            {
                entity.ToCollection("Calculations");
                entity.Property(e => e.Id)
                  .HasConversion(v => v.ToString(), v => Guid.Parse(v));
            }

            entity.HasKey(x => x.Id);
        });
    }
}