using CSharpFunctionalExtensions;

namespace Applications.Application.Domain.Application;

public record CustomerPersonalData
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Pesel { get; init; }
}
