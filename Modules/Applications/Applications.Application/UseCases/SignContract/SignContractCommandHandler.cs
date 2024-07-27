using Applications.Application.Infrastructure.Database;
using MediatR;
using Wolverine;
using Wolverine.Attributes;
using static Applications.Application.UseCases.SignContract.SignContractCommandResponse;


namespace Applications.Application.UseCases.SignContract;

[MessageIdentity("contractSigned", Version = 1)]
public record ContractSigned(string ApplicationId);

internal class SignContractCommandHandler(
    CreditApplicationDbContext creditApplicationDbContext,
    IMessageBus publishEndpoint,
    TimeProvider timeProvider
    ) : IRequestHandler<SignContractCommand, SignContractCommandResponse>
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext = creditApplicationDbContext;
    private readonly IMessageBus _publishEndpoint = publishEndpoint;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<SignContractCommandResponse> Handle(SignContractCommand command, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.GetCreditApplicationAsync(command.ApplicationId);
        if (creditApplication is null)
        {
            return ResourceNotFound.Result;
        }

        creditApplication.SignContract(_timeProvider);

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.PublishAsync(new ContractSigned(creditApplication.Id));

        return OK.Result;
    }
}
