using Common.Application.Dictionary;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Calculations.Application.Domain;

public class CreditCalculation
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string ApplicationId { get; set; }

    public decimal Amount { get; set; }

    public int CreditPeriodInMonths { get; set; }

    public Decision Decision { get; set; }
}
