using Applications.Application.Infrastructure.Database;
using Common.Application.Errors;
using Common.Kafka;
using CSharpFunctionalExtensions;
using MediatR;

namespace Applications.Application.UseCases.SignContract;

[EventEnvelope(Topic = "event.credit.applications.contractSigned.v1")]
public record ContractSigned(string ApplicationId) : INotification;

internal class SignContractCommandHandler(
    CreditApplicationDbContext creditApplicationDbContext,
    IEventBusProducer eventBusProducer,
    TimeProvider timeProvider
    ) : IRequestHandler<SignContractCommand, Result>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext = creditApplicationDbContext;
    private readonly IEventBusProducer _eventBusProducer = eventBusProducer;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result> Handle(SignContractCommand command, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(command.ApplicationId);
        if (creditApplication is null)
            return Result.Failure(ErrorCode.ResourceNotFound);

        creditApplication.SignContract(timeProvider);

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _eventBusProducer.PublishAsync(new ContractSigned(creditApplication.ApplicationId), cancellationToken);

        return Result.Success();
    }
}
