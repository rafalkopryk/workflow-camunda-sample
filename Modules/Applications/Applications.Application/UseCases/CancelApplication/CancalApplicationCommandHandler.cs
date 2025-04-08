using Applications.Application.Infrastructure.Database;
using Applications.Application.UseCases.CloseApplication;
using Common.Application.Cqrs;
using Wolverine;
using static Applications.Application.UseCases.CancelApplication.CancelApplicationCommandResponse;

namespace Applications.Application.UseCases.CancelApplication;

internal class CancalApplicationCommandHandler(
    CreditApplicationDbContext creditApplicationDbContext,
    IMessageBus eventBusProducer,
    TimeProvider timeProvider
    ) : IRequestHandler<CancelApplicationCommand, CancelApplicationCommandResponse>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext = creditApplicationDbContext;
    private readonly IMessageBus _eventBusProducer = eventBusProducer;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<CancelApplicationCommandResponse> Handle(CancelApplicationCommand command, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(command.ApplicationId);
        if (creditApplication is null)
        {
            return ResourceNotFound.Result;
        }

        creditApplication.CloseApplication(_timeProvider);

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _eventBusProducer.PublishAsync(new ApplicationClosed(creditApplication.Id), new DeliveryOptions
        {
            PartitionKey = creditApplication.Id
        });

        return OK.Result;
    }
}
