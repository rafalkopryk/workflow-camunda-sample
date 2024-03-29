﻿using Applications.Application.Domain.Application;
using Microsoft.EntityFrameworkCore;

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
        modelBuilder.Entity<CreditApplication>(entity =>
        {
            entity.ToTable("CreditApplication", c => c.IsTemporal());
            entity.HasKey(creditApplication => creditApplication.ApplicationId);

            entity.OwnsOne(creditApplication => creditApplication.CustomerPersonalData, ownedNavigationBuilder => ownedNavigationBuilder.ToJson());
            entity.OwnsOne(creditApplication => creditApplication.Declaration, ownedNavigationBuilder => ownedNavigationBuilder.ToJson());
            entity.OwnsMany(creditApplication => creditApplication.States, ownedNavigationBuilder  => ownedNavigationBuilder.ToJson());
        });
    }
}
