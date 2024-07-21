using Applications.Application.Domain.Application;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Applications.Application.Infrastructure.Database;

public class CreditApplicationDbContext : DbContext
{
    public DbSet<CreditApplication> Applications { get; set; }

    public CreditApplicationDbContext(DbContextOptions<CreditApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (Database.IsCosmos())
        {
            modelBuilder.HasManualThroughput(400);
        }

        modelBuilder.Entity<CreditApplication>(entity =>
        {
            if (Database.IsCosmos())
            {
                entity.ToContainer("CreditApplication");
                entity.HasNoDiscriminator();
            }

            if (Database.IsSqlServer())
            {
                entity.ToTable("CreditApplication");
            }
            
            if (Database.ProviderName == "MongoDB.EntityFrameworkCore")
            {
                entity.ToCollection("CreditApplication");
            }

            entity.HasKey(creditApplication => creditApplication.Id);
            
            entity.OwnsOne(creditApplication => creditApplication.CustomerPersonalData, ownedNavigationBuilder => ownedNavigationBuilder.ToJson());
            entity.OwnsOne(creditApplication => creditApplication.Declaration, ownedNavigationBuilder => ownedNavigationBuilder.ToJson());
            entity.OwnsMany(creditApplication => creditApplication.States, ownedNavigationBuilder => ownedNavigationBuilder.ToJson());
        });
    }
}
