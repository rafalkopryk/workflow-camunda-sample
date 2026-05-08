using Calculations.Application.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Calculations.WebApi;

// Design-time-only subclass used by Aspire.Hosting.EntityFrameworkCore / dotnet-ef
// to scaffold and apply SqlServer migrations. Never registered in DI at runtime.
public class CreditCalculationDbContextSqlServer : CreditCalculationDbContext
{
    public CreditCalculationDbContextSqlServer(DbContextOptions<CreditCalculationDbContext> options) : base(options) { }

    public class Factory : IDesignTimeDbContextFactory<CreditCalculationDbContextSqlServer>
    {
        public CreditCalculationDbContextSqlServer CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                ?? "Server=localhost;Database=design;User Id=design;Password=design;TrustServerCertificate=True";
            var options = new DbContextOptionsBuilder<CreditCalculationDbContext>()
                .UseSqlServer(connectionString, b => b.MigrationsAssembly("Calculations.WebApi"))
                .Options;
            return new CreditCalculationDbContextSqlServer(options);
        }
    }
}
