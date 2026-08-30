using Applications.Application.Infrastructure.Database;
using Applications.Contracts.Events;
using Common.Application.Cqrs;
using Wolverine;
using static Applications.Application.Features.SignContract.SignContractCommandResponse;

namespace Applications.Application.Features.SignContract;

internal class SignContractCommandHandler(
    CreditApplicationDbContext creditApplicationDbContext,
    IMessageBus publishEndpoint,
    TimeProvider timeProvider
    ) : IRequestHandler<SignContractCommand, SignContractCommandResponse>
{
    public async Task<SignContractCommandResponse> Handle(SignContractCommand command, CancellationToken cancellationToken)
    {
        var creditApplication = await creditApplicationDbContext.GetCreditApplicationAsync(command.ApplicationId);
        if (creditApplication is null)
        {
            return ResourceNotFound.Result;
        }

        creditApplication.SignContract(timeProvider);

        await creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await publishEndpoint.PublishAsync(new ContractSigned(creditApplication.Id), new DeliveryOptions
        {
            PartitionKey = creditApplication.Id
        });

        return OK.Result;
    }
}
