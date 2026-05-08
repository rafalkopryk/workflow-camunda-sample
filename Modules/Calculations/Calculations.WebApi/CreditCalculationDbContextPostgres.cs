using Calculations.Application.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Calculations.WebApi;

// Design-time-only subclass used by Aspire.Hosting.EntityFrameworkCore / dotnet-ef
// to scaffold and apply Postgres migrations. Never registered in DI at runtime.
public class CreditCalculationDbContextPostgres : CreditCalculationDbContext
{
    public CreditCalculationDbContextPostgres(DbContextOptions<CreditCalculationDbContext> options) : base(options) { }

    public class Factory : IDesignTimeDbContextFactory<CreditCalculationDbContextPostgres>
    {
        public CreditCalculationDbContextPostgres CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
                ?? "Host=localhost;Database=design;Username=design;Password=design";
            var options = new DbContextOptionsBuilder<CreditCalculationDbContext>()
                .UseNpgsql(connectionString, b => b.MigrationsAssembly("Calculations.WebApi"))
                .Options;
            return new CreditCalculationDbContextPostgres(options);
        }
    }
}
