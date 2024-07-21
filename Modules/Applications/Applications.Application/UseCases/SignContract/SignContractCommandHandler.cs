using Applications.Application.Infrastructure.Database;
using Common.Application.Errors;
using CSharpFunctionalExtensions;
using MediatR;
using Wolverine;
using Wolverine.Attributes;

namespace Applications.Application.UseCases.SignContract;

[MessageIdentity("contractSigned", Version = 1)]
public record ContractSigned(string ApplicationId);

internal class SignContractCommandHandler(
    CreditApplicationDbContext creditApplicationDbContext,
    IMessageBus publishEndpoint,
    TimeProvider timeProvider
    ) : IRequestHandler<SignContractCommand, Result>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext = creditApplicationDbContext;
    private readonly IMessageBus _publishEndpoint = publishEndpoint;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<Result> Handle(SignContractCommand command, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(command.ApplicationId);
        if (creditApplication is null)
            return Result.Failure(ErrorCode.ResourceNotFound);

        creditApplication.SignContract(_timeProvider);

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.PublishAsync(new ContractSigned(creditApplication.Id));

        return Result.Success();
    }
}
