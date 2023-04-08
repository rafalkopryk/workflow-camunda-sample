using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Common.Application.Errors;
using CSharpFunctionalExtensions;
using MediatR;

namespace Applications.Application.UseCases.GetApplication;

internal class GetApplicationQuerydHandler : IRequestHandler<GetApplicationQuery, Result<GetApplicationQueryResponse>>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext;

    public GetApplicationQuerydHandler(CreditApplicationDbContext creditApplicationDbContext)
    {
        _creditApplicationDbContext = creditApplicationDbContext;
    }

    public async Task<Result<GetApplicationQueryResponse>> Handle(GetApplicationQuery query, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(query.ApplicationId);

        if (creditApplication is null)
            return Result.Failure<GetApplicationQueryResponse>(ErrorCode.ResourceNotFound);

        return Map(creditApplication);
    }

    private static GetApplicationQueryResponse Map(CreditApplication creditApplication)
    {
        return new GetApplicationQueryResponse
        {
            CreditApplication = new()
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
                    Level = creditApplication.State.Level,
                    Decision = creditApplication.State.Decision,
                    ContractSigningDate = creditApplication.State.ContractSigningDate,
                    Date = creditApplication.State.Date,
                }
            }
        };
    }
}
