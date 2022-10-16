using Applications.Application.UseCases.GetApplication.Dto;

namespace Applications.Application.UseCases.GetApplication;

public record GetApplicationQueryResponse
{
    public GetApplicationQueryCreditApplicationDto CreditApplication { get; init; }
}
