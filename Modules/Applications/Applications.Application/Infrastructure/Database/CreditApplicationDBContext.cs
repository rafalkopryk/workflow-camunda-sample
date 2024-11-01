using System.Text.Json;
using Applications.Application.Domain.Application;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
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
                // entity.Property(x => x.States).HasConversion<byte[]>(
                //     x => SerializeToBson(x),
                //     y => DeserializeFromBson(y));
            }

            entity.HasKey(creditApplication => creditApplication.Id);
            
            entity.OwnsOne(creditApplication => creditApplication.CustomerPersonalData, ownedNavigationBuilder => ownedNavigationBuilder.ToJson());
            entity.OwnsOne(creditApplication => creditApplication.Declaration, ownedNavigationBuilder => ownedNavigationBuilder.ToJson());
            entity.Property(x => x.States).HasConversion<string>(
                x => JsonSerializer.Serialize(x, JsonSerializerOptions.Web),
                y => JsonSerializer.Deserialize<ApplicationStates>(y, JsonSerializerOptions.Web));
        });
    }
    
    private static byte[] SerializeToBson(ApplicationState[] states)
    {
        using var memoryStream = new MemoryStream();
        using (var bsonWriter = new BsonBinaryWriter(memoryStream))
        {
            var document = new BsonDocument { { "States", BsonArray.Create(states) } };
            BsonSerializer.Serialize(bsonWriter, document);
        }
        return memoryStream.ToArray();
    }

    private static ApplicationState[] DeserializeFromBson(byte[] bsonData)
    {
        using var memoryStream = new MemoryStream(bsonData);
        using var bsonReader = new BsonBinaryReader(memoryStream);
    
        var document = BsonSerializer.Deserialize<BsonDocument>(bsonReader);
        return document["States"].AsBsonArray.Select(value => BsonSerializer.Deserialize<ApplicationState>(value.AsBsonDocument)).ToArray();
    }
}


