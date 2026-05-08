using Applications.Application.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Applications.WebApi;

// Design-time-only subclass used by Aspire.Hosting.EntityFrameworkCore / dotnet-ef
// to scaffold and apply Postgres migrations. Never registered in DI at runtime.
public class CreditApplicationDbContextPostgres : CreditApplicationDbContext
{
    public CreditApplicationDbContextPostgres(DbContextOptions<CreditApplicationDbContext> options) : base(options) { }

    public class Factory : IDesignTimeDbContextFactory<CreditApplicationDbContextPostgres>
    {
        public CreditApplicationDbContextPostgres CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
                ?? "Host=localhost;Database=design;Username=design;Password=design";
            var options = new DbContextOptionsBuilder<CreditApplicationDbContext>()
                .UseNpgsql(connectionString, b => b.MigrationsAssembly("Applications.WebApi"))
                .Options;
            return new CreditApplicationDbContextPostgres(options);
        }
    }
}
