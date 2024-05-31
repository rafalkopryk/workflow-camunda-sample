using Applications.Application.Infrastructure.Database;
using Common.Application;
using Common.Application.Errors;
using CSharpFunctionalExtensions;
using MassTransit;
using MediatR;

namespace Applications.Application.UseCases.SignContract;

[EntityName("event.credit.applications.contractSigned.v1")]
[MessageUrn("event.credit.applications.contractSigned.v1")]
public record ContractSigned(string ApplicationId) : INotification;

internal class SignContractCommandHandler(
    CreditApplicationDbContext creditApplicationDbContext,
    BusProxy publishEndpoint,
    TimeProvider timeProvider
    ) : IRequestHandler<SignContractCommand, Result>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext = creditApplicationDbContext;
    private readonly BusProxy _publishEndpoint = publishEndpoint;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result> Handle(SignContractCommand command, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(command.ApplicationId);
        if (creditApplication is null)
            return Result.Failure(ErrorCode.ResourceNotFound);

        creditApplication.SignContract(_timeProvider);

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new ContractSigned(creditApplication.ApplicationId), cancellationToken);

        return Result.Success();
    }
}
