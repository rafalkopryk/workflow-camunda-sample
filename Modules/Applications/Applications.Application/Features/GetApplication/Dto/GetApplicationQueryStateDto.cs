using Common.Application.Dictionary;

namespace Applications.Application.Features.GetApplication.Dto;

public record GetApplicationStateDto
{
    public ApplicationLevel Level { get; init; }

    public DateTimeOffset Date { get; init; }

    public DateTimeOffset? ContractSigningDate { get; init; }

    public Decision Decision { get; init; }
}