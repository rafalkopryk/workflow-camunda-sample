using Applications.Application.Infrastructure.Database;
using Applications.Application.UseCases.CloseApplication;
using Common.Application.Errors;
using Common.Kafka;
using CSharpFunctionalExtensions;
using MediatR;

namespace Applications.Application.UseCases.CancelApplication;

internal class CancalApplicationCommandHandler(
    CreditApplicationDbContext creditApplicationDbContext,
    IEventBusProducer eventBusProducer,
    TimeProvider timeProvider
    ) : IRequestHandler<CancelApplicationCommand, Result>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext = creditApplicationDbContext;
    private readonly IEventBusProducer _eventBusProducer = eventBusProducer;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result> Handle(CancelApplicationCommand command, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(command.ApplicationId);
        if (creditApplication is null)
            return Result.Failure(ErrorCode.ResourceNotFound);

        creditApplication.CloseApplication(_timeProvider);

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _eventBusProducer.PublishAsync(new ApplicationClosed(creditApplication.ApplicationId), cancellationToken);

        return Result.Success();
    }
}
