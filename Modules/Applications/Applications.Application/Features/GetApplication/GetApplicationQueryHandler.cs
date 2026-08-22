using Applications.Application.Domain.Application;
using Applications.Application.Features.GetApplication.Dto;
using Applications.Application.Infrastructure.Database;
using Common.Application.Cqrs;
using static Applications.Application.Features.GetApplication.GetApplicationQueryResponse;

namespace Applications.Application.Features.GetApplication;

internal class GetApplicationQueryHandler(CreditApplicationDbContext creditApplicationDbContext) : IRequestHandler<GetApplicationQuery, GetApplicationQueryResponse>
{
    public async Task<GetApplicationQueryResponse> Handle(GetApplicationQuery query, CancellationToken cancellationToken)
    {
        var creditApplication = await creditApplicationDbContext.GetCreditApplicationAsync(query.ApplicationId);

        if (creditApplication is null)
        {
            return ResourceNotFound.Result;
        }

        var result = Map(creditApplication);
        
        return new OK(result);
    }

    private static GetApplicationQueryCreditApplicationDto Map(CreditApplication creditApplication)
    {
        return new GetApplicationQueryCreditApplicationDto
        {
            Amount = creditApplication.Amount,
            CreditPeriodInMonths = creditApplication.CreditPeriodInMonths,
            CustomerPersonalData = new()
            {
                FirstName = creditApplication.CustomerPersonalData.FirstName,
                LastName = creditApplication.CustomerPersonalData.LastName,
                DocumentId = creditApplication.CustomerPersonalData.DocumentId,
            },
            Declaration = new()
            {
                AverageNetMonthlyIncome = creditApplication.Declaration.AverageNetMonthlyIncome,
            },
            State = new()
            {
                Level = creditApplication.States.Current.Level,
                Decision = creditApplication.States.Current.Decision,
                ContractSigningDate = creditApplication.States.ContractSigningDate,
                Date = creditApplication.States.Current.Date,
            }
        };
    }
}
