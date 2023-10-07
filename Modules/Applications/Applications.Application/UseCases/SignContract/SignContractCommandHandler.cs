using Applications.Application.Infrastructure.Database;
using Common.Application.Errors;
using Common.Kafka;
using CSharpFunctionalExtensions;
using MediatR;

namespace Applications.Application.UseCases.SignContract;

[EventEnvelope(Topic = "event.credit.applications.contractSigned.v1")]
public record ContractSigned(string ApplicationId) : INotification;

internal class SignContractCommandHandler : IRequestHandler<SignContractCommand, Result>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext;
    private readonly IEventBusProducer _eventBusProducer;

    public SignContractCommandHandler(CreditApplicationDbContext creditApplicationDbContext, IEventBusProducer eventBusProducer)
    {
        _creditApplicationDbContext = creditApplicationDbContext;
        _eventBusProducer = eventBusProducer;
    }

    public async Task<Result> Handle(SignContractCommand command, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(command.ApplicationId);
        if (creditApplication is null)
            return Result.Failure(ErrorCode.ResourceNotFound);

        creditApplication.SignContract();

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _eventBusProducer.PublishAsync(new ContractSigned(creditApplication.ApplicationId), cancellationToken);

        return Result.Success();
    }
}
