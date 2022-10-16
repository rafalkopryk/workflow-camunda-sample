using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Common.Application.Zeebe;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Applications.Application.UseCases.GetApplication;

internal class GetApplicationQuerydHandler : IRequestHandler<GetApplicationQuery, GetApplicationQueryResponse>
{
    private readonly IZeebeService _processManager;
    private readonly CreditApplicationDbContext _creditApplicationDbContext;

    public GetApplicationQuerydHandler(IZeebeService zeebeService, CreditApplicationDbContext creditApplicationDbContext)
    {
        _processManager = zeebeService;
        _creditApplicationDbContext = creditApplicationDbContext;
    }

    public async Task<GetApplicationQueryResponse> Handle(GetApplicationQuery query, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.Applications.AsNoTracking()
            .FirstAsync(application => application.ApplicationId == query.ApplicationId, cancellationToken);

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
