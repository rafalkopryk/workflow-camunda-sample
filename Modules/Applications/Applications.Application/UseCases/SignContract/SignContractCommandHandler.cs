using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Common.Application.Errors;
using CSharpFunctionalExtensions;
using GatewayProtocol;
using MediatR;

namespace Applications.Application.UseCases.SignContract;

internal class SignContractCommandHandler : IRequestHandler<SignContractCommand, Result>
{
    private readonly Gateway.GatewayClient _client;
    private readonly CreditApplicationDbContext _creditApplicationDbContext;

    public SignContractCommandHandler(Gateway.GatewayClient client, CreditApplicationDbContext creditApplicationDbContext)
    {
        _client = client;
        _creditApplicationDbContext = creditApplicationDbContext;
    }

    public async Task<Result> Handle(SignContractCommand command, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.Applications.FindAsync(command.ApplicationId);
        if (creditApplication is null)
            return Result.Failure(ErrorCode.ResourceNotFound);

        creditApplication.ForwardTo(State.ContractSigned(creditApplication.State, DateTimeOffset.Now));

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _client.PublishMessageAsync(new PublishMessageRequest
        {
            Name = "contract-signed",
            CorrelationKey = creditApplication.ApplicationId.ToString()
        });

        return Result.Success();
    }
}
