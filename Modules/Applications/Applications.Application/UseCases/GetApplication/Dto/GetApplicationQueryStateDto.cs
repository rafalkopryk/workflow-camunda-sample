using Common.Application.Dictionary;

namespace Applications.Application.UseCases.GetApplication.Dto;

public record GetApplicationStateDto
{
    public string Level { get; init; }

    public DateTimeOffset Date { get; init; }

    public DateTimeOffset? ContractSigningDate { get; init; }

    public Decision Decision { get; init; }
}