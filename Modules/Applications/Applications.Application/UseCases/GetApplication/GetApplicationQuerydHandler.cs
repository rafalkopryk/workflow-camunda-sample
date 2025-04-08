using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Applications.Application.UseCases.GetApplication.Dto;
using Common.Application.Cqrs;
using static Applications.Application.UseCases.GetApplication.GetApplicationQueryResponse;

namespace Applications.Application.UseCases.GetApplication;

internal class GetApplicationQuerydHandler(CreditApplicationDbContext creditApplicationDbContext) : IRequestHandler<GetApplicationQuery, GetApplicationQueryResponse>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext = creditApplicationDbContext;

    public async Task<GetApplicationQueryResponse> Handle(GetApplicationQuery query, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(query.ApplicationId);

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
                Pesel = creditApplication.CustomerPersonalData.Pesel,
            },
            Declaration = new()
            {
                AverageNetMonthlyIncome = creditApplication.Declaration.AverageNetMonthlyIncome,
            },
            State = new()
            {
                Level = creditApplication.States.Current.Level,
                Decision = creditApplication.States.Current.Decision,
                ContractSigningDate = creditApplication.States.History.OfType<ApplicationState.ContractSigned>().FirstOrDefault()?.Date,
                Date = creditApplication.States.Current.Date,
            }
        };
    }
}
