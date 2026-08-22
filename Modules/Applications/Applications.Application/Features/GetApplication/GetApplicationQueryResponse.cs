using Applications.Application.Features.GetApplication.Dto;

namespace Applications.Application.Features.GetApplication;

public abstract record GetApplicationQueryResponse 
{
    public record OK(GetApplicationQueryCreditApplicationDto CreditApplication) : GetApplicationQueryResponse;

    public record ResourceNotFound() : GetApplicationQueryResponse
    {
        public static readonly ResourceNotFound Result = new();
    }
}
