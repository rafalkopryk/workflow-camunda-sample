using Applications.Application.Infrastructure.Database;
using Applications.Application.UseCases.CloseApplication;
using Common.Application.Errors;
using Common.Kafka;
using CSharpFunctionalExtensions;
using MediatR;

namespace Applications.Application.UseCases.CancelApplication;

internal class CancalApplicationCommandHandler : IRequestHandler<CancelApplicationCommand, Result>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext;
    private readonly IEventBusProducer _eventBusProducer;

    public CancalApplicationCommandHandler(CreditApplicationDbContext creditApplicationDbContext, IEventBusProducer eventBusProducer)
    {
        _creditApplicationDbContext = creditApplicationDbContext;
        _eventBusProducer = eventBusProducer;
    }

    public async Task<Result> Handle(CancelApplicationCommand command, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(command.ApplicationId);
        if (creditApplication is null)
            return Result.Failure(ErrorCode.ResourceNotFound);

        creditApplication.CloseApplication();

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _eventBusProducer.PublishAsync(new ApplicationClosed(creditApplication.ApplicationId), cancellationToken);

        return Result.Success();
    }
}
