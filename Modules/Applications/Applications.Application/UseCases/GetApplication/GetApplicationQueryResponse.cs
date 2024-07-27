using Applications.Application.UseCases.GetApplication.Dto;

namespace Applications.Application.UseCases.GetApplication;

public abstract record GetApplicationQueryResponse 
{
    public record OK(GetApplicationQueryCreditApplicationDto CreditApplication) : GetApplicationQueryResponse;

    public record ResourceNotFound() : GetApplicationQueryResponse
    {
        public static readonly ResourceNotFound Result = new();
    }
}
