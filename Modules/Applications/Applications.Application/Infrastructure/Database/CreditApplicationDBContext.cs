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

            if (Database.IsNpgsql())
            {
                entity.ToTable("CreditApplication");
            }

            var isMongoDb = Database.ProviderName == "MongoDB.EntityFrameworkCore";
            if (isMongoDb)
            {
                entity.ToCollection("CreditApplication");
            }

            entity.HasKey(creditApplication => creditApplication.Id);

            if (isMongoDb)
            {
                entity.OwnsOne(application => application.CustomerPersonalData)
                    .HasElementName(nameof(CreditApplication.CustomerPersonalData));
                entity.OwnsOne(application => application.Declaration)
                    .HasElementName(nameof(CreditApplication.Declaration));
                entity.OwnsMany(application => application.States)
                    .HasElementName(nameof(CreditApplication.States));
            }
            else
            {
                entity.ComplexProperty(
                    application => application.CustomerPersonalData,
                    complexProperty => complexProperty.ToJson(nameof(CreditApplication.CustomerPersonalData)));
                entity.ComplexProperty(
                    application => application.Declaration,
                    complexProperty => complexProperty.ToJson(nameof(CreditApplication.Declaration)));
                entity.ComplexCollection(
                    application => application.States,
                    complexCollection => complexCollection.ToJson(nameof(CreditApplication.States)));
            }
        });
    }
}

