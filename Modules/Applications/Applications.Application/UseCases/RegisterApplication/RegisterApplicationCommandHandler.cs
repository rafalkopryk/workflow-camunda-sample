using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Common.Application.Zeebe;
using MediatR;

namespace Applications.Application.UseCases.RegisterApplication;

internal class RegisterApplicationCommandHandler : IRequestHandler<RegisterApplicationCommand>
{
    private readonly IZeebeService _processManager;
    private readonly CreditApplicationDbContext _creditApplicationDbContext;

    public RegisterApplicationCommandHandler(IZeebeService zeebeService, CreditApplicationDbContext creditApplicationDbContext)
    {
        _processManager = zeebeService;
        _creditApplicationDbContext = creditApplicationDbContext;
    }

    public async Task<Unit> Handle(RegisterApplicationCommand command, CancellationToken cancellationToken)
    {
        var creditApplication = CreateCreditApplication(command);

        await _creditApplicationDbContext.AddAsync(creditApplication, cancellationToken);
        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _processManager.StartProcessInstance(
            "credit-application",
            new
            {
                creditApplication.ApplicationId,
                creditApplication.Amount,
                creditApplication.CreditPeriodInMonths,
                creditApplication.Declaration.AverageNetMonthlyIncome,
            });

        return Unit.Value;
    }

    private static CreditApplication CreateCreditApplication(RegisterApplicationCommand request)
    {
        return CreditApplication.Create(
            request.ApplicationId,
            request.CreditApplication.Amount,
            request.CreditApplication.CreditPeriodInMonths,
            new CustomerPersonalData
            {
                FirstName = request.CreditApplication.CustomerPersonalData.FirstName,
                LastName = request.CreditApplication.CustomerPersonalData.LastName,
                Pesel = request.CreditApplication.CustomerPersonalData.Pesel,
            },
            new Declaration
            {
                AverageNetMonthlyIncome = request.CreditApplication.Declaration.AverageNetMonthlyIncome
            },
            DateTimeOffset.Now);
    }
}
