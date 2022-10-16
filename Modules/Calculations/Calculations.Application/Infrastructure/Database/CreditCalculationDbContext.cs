using Calculations.Application.Domain;
using Microsoft.EntityFrameworkCore;

namespace Calculations.Application.Infrastructure.Database;

internal class CreditCalculationDbContext : DbContext
{
    public DbSet<CreditCalculation> Calculations { get; set; }

    public CreditCalculationDbContext(DbContextOptions<CreditCalculationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CreditCalculation>(entity =>
        {
            entity.ToTable("CreditCalculation");
            entity.HasKey(creditApplication => creditApplication.CalcualtionId);
        });
    }
}
