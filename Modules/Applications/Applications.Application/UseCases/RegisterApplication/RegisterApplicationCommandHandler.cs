using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Camunda.Client;
using Common.Application.Errors;
using CSharpFunctionalExtensions;
using MediatR;

namespace Applications.Application.UseCases.RegisterApplication;

[ZeebeMessage(Name = "Message_ApplicationRegistered", TimeToLiveInMs = 24 * 3600 * 1000)]
public record ApplicationRegistered(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);

internal class RegisterApplicationCommandHandler(
    IMessageClient client,
    CreditApplicationDbContext creditApplicationDbContext,
    TimeProvider timeProvider
    ) : IRequestHandler<RegisterApplicationCommand, Result>
{
    private readonly IMessageClient _client = client;
    private readonly CreditApplicationDbContext _creditApplicationDbContext = creditApplicationDbContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result> Handle(RegisterApplicationCommand command, CancellationToken cancellationToken)
    {
        if (await _creditApplicationDbContext.HasCreditApplicationAsync(command.ApplicationId))
        {
            return Result.Failure(ErrorCode.ResourceExists);
        }

        var creditApplication = CreateCreditApplication(command);

        await _creditApplicationDbContext.AddAsync(creditApplication, cancellationToken);

        await _client.Publish(
            null!,
            new ApplicationRegistered(
                creditApplication.ApplicationId,
                creditApplication.Amount,
                creditApplication.CreditPeriodInMonths,
                creditApplication.Declaration.AverageNetMonthlyIncome),
            creditApplication.ApplicationId);

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

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
