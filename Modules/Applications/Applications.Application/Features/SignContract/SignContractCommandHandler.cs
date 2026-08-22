using Applications.Application.Infrastructure.Database;
using Common.Application.Cqrs;
using Wolverine;
using Wolverine.Attributes;
using static Applications.Application.Features.SignContract.SignContractCommandResponse;

namespace Applications.Application.Features.SignContract;

[MessageIdentity("contractSigned", Version = 1)]
public record ContractSigned(string ApplicationId);

internal class SignContractCommandHandler(
    CreditApplicationDbContext creditApplicationDbContext,
    IMessageBus publishEndpoint,
    TimeProvider timeProvider
    ) : IRequestHandler<SignContractCommand, SignContractCommandResponse>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext = creditApplicationDbContext;

    public async Task<SignContractCommandResponse> Handle(SignContractCommand command, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(command.ApplicationId);
        if (creditApplication is null)
        {
            return ResourceNotFound.Result;
        }

        creditApplication.SignContract(timeProvider);

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await publishEndpoint.PublishAsync(new ContractSigned(creditApplication.Id), new DeliveryOptions
        {
            PartitionKey = creditApplication.Id
        });

        return OK.Result;
    }
}
