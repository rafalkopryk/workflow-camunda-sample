using Applications.Application.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Applications.WebApi;

// Design-time-only subclass used by Aspire.Hosting.EntityFrameworkCore / dotnet-ef
// to scaffold and apply SqlServer migrations. Never registered in DI at runtime.
public class CreditApplicationDbContextSqlServer : CreditApplicationDbContext
{
    public CreditApplicationDbContextSqlServer(DbContextOptions<CreditApplicationDbContext> options) : base(options) { }

    public class Factory : IDesignTimeDbContextFactory<CreditApplicationDbContextSqlServer>
    {
        public CreditApplicationDbContextSqlServer CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                ?? "Server=localhost;Database=design;User Id=design;Password=design;TrustServerCertificate=True";
            var options = new DbContextOptionsBuilder<CreditApplicationDbContext>()
                .UseSqlServer(connectionString, b => b.MigrationsAssembly("Applications.WebApi"))
                .Options;
            return new CreditApplicationDbContextSqlServer(options);
        }
    }
}
