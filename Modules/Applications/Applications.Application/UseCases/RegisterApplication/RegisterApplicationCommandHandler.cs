using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Common.Application.Errors;
using CSharpFunctionalExtensions;
using MediatR;
using Wolverine;
using Wolverine.Attributes;

namespace Applications.Application.UseCases.RegisterApplication;

[MessageIdentity("applicationRegistered", Version = 1)]
public record ApplicationRegistered(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);

internal class RegisterApplicationCommandHandler(
    IMessageBus bus,
    CreditApplicationDbContext creditApplicationDbContext,
    TimeProvider timeProvider
    ) : IRequestHandler<RegisterApplicationCommand, Result>
{
    private readonly IMessageBus _bus = bus;
    private readonly CreditApplicationDbContext _creditApplicationDbContext = creditApplicationDbContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result> Handle(RegisterApplicationCommand command, CancellationToken cancellationToken)
    {
        if (await _creditApplicationDbContext.HasCreditApplicationAsync(command.ApplicationId))
        {
            return Result.Failure(ErrorCode.ResourceExists);
        }

        var creditApplication = CreateCreditApplication(command);

        await _creditApplicationDbContext.AddAsync(creditApplication, cancellationToken);;

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _bus.PublishAsync(
            new ApplicationRegistered(
                creditApplication.ApplicationId,
                creditApplication.Amount,
                creditApplication.CreditPeriodInMonths,
                creditApplication.Declaration.AverageNetMonthlyIncome));

        return Result.Success();
    }

    private CreditApplication CreateCreditApplication(RegisterApplicationCommand request)
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
            _timeProvider);
    }
}
