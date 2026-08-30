using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Applications.Contracts.Events;
using Common.Application.Cqrs;
using Wolverine;
using static Applications.Application.Features.RegisterApplication.RegisterApplicationCommandResponse;

namespace Applications.Application.Features.RegisterApplication;

internal class RegisterApplicationCommandHandler(
    IMessageBus bus,
    CreditApplicationDbContext creditApplicationDbContext,
    TimeProvider timeProvider
    ) : IRequestHandler<RegisterApplicationCommand, RegisterApplicationCommandResponse>
{
    public async Task<RegisterApplicationCommandResponse> Handle(RegisterApplicationCommand command, CancellationToken cancellationToken)
    {
        if (await creditApplicationDbContext.HasCreditApplicationAsync(command.ApplicationId))
        {
            return ResourceExists.Result;
        }

        var creditApplication = CreateCreditApplication(command);

        await creditApplicationDbContext.AddAsync(creditApplication, cancellationToken); ;

        await creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        var deliveryOptions = new DeliveryOptions{ PartitionKey = creditApplication.Id };
        var task = command.ProcessCode switch
        {
            _ => bus.PublishAsync(new ApplicationRegistered(
                creditApplication.Id,
                creditApplication.Amount,
                creditApplication.CreditPeriodInMonths,
                creditApplication.Declaration.AverageNetMonthlyIncome,
                creditApplication.CustomerPersonalData.DocumentId), deliveryOptions)
        };

        await task;

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
                request.CreditApplication.CustomerPersonalData.DocumentId),
            new Declaration
            (
              request.CreditApplication.Declaration.AverageNetMonthlyIncome
            ),
            timeProvider);
    }
}
