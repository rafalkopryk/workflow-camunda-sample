﻿using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using MediatR;
using Wolverine;
using Wolverine.Attributes;
using static Applications.Application.UseCases.RegisterApplication.RegisterApplicationCommandResponse;


namespace Applications.Application.UseCases.RegisterApplication;

[MessageIdentity("applicationRegistered", Version = 1)]
public record ApplicationRegistered(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome);

internal class RegisterApplicationCommandHandler(
    IMessageBus bus,
    CreditApplicationDbContext creditApplicationDbContext,
    TimeProvider timeProvider
    ) : IRequestHandler<RegisterApplicationCommand, RegisterApplicationCommandResponse>
{
    private readonly IMessageBus _bus = bus;
    private readonly CreditApplicationDbContext _creditApplicationDbContext = creditApplicationDbContext;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<RegisterApplicationCommandResponse> Handle(RegisterApplicationCommand command, CancellationToken cancellationToken)
    {
        if (await _creditApplicationDbContext.HasCreditApplicationAsync(command.ApplicationId))
        {
            return ResourceExists.Result;
        }

        var creditApplication = CreateCreditApplication(command);

        await _creditApplicationDbContext.AddAsync(creditApplication, cancellationToken);;

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _bus.PublishAsync(
            new ApplicationRegistered(
                creditApplication.Id,
                creditApplication.Amount,
                creditApplication.CreditPeriodInMonths,
                creditApplication.Declaration.AverageNetMonthlyIncome));

        return OK.Result;
    }

    private CreditApplication CreateCreditApplication(RegisterApplicationCommand request)
    {
        return CreditApplication.Create(
            request.ApplicationId,
            request.CreditApplication.Amount,
            request.CreditApplication.CreditPeriodInMonths,
            new CustomerPersonalData(
                request.CreditApplication.CustomerPersonalData.FirstName,
                request.CreditApplication.CustomerPersonalData.LastName,
                request.CreditApplication.CustomerPersonalData.Pesel),
            new Declaration
            (
              request.CreditApplication.Declaration.AverageNetMonthlyIncome
            ),
            _timeProvider);
    }
}
