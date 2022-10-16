using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Common.Application.Zeebe;
using MediatR;

namespace Applications.Application.UseCases.SignContract;

internal class SignContractCommandHandler : IRequestHandler<SignContractCommand>
{
    private readonly IZeebeService _processManager;
    private readonly CreditApplicationDbContext _creditApplicationDbContext;

    public SignContractCommandHandler(IZeebeService zeebeService, CreditApplicationDbContext creditApplicationDbContext)
    {
        _processManager = zeebeService;
        _creditApplicationDbContext = creditApplicationDbContext;
    }

    public async Task<Unit> Handle(SignContractCommand command, CancellationToken cancellationToken)
    {
        var creditApplication = await _creditApplicationDbContext.Applications.FindAsync(command.ApplicationId);
        creditApplication.ForwardTo(State.ContractSigned(creditApplication.State, DateTimeOffset.Now));

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _processManager.PublishMessage("contract-signed", creditApplication.ApplicationId.ToString(), cancellationToken);

        return Unit.Value;
    }
}
