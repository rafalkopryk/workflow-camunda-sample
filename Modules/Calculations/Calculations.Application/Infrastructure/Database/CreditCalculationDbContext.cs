using Calculations.Application.Domain;
using Microsoft.EntityFrameworkCore;

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

            entity.HasKey(creditApplication => creditApplication.CalcualtionId);
        });
    }
}