using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Common.Application.Dictionary;
using Common.Application.Serializer;
using Common.Application.Zeebe;
using MediatR;
using System.Text.Json;

namespace Applications.Application.UseCases.SetDecision;

internal class SetDecisionCommandCommandHandler : IRequestHandler<SetDecisionCommand>
{
    private readonly IZeebeService _zeebeService;
    private readonly CreditApplicationDbContext _creditApplicationDbContext;

    public SetDecisionCommandCommandHandler(IZeebeService zeebeService, CreditApplicationDbContext creditApplicationDbContext)
    {
        _zeebeService = zeebeService;
        _creditApplicationDbContext = creditApplicationDbContext;
    }

    public async Task<Unit> Handle(SetDecisionCommand command, CancellationToken cancellationToken)
    {
        var input = JsonSerializer.Deserialize<Input>(command.Job.Variables, JsonSerializerCustomOptions.CamelCase);

        var creditApplication = await _creditApplicationDbContext.Applications.FindAsync(input.ApplicationId);
        creditApplication.ForwardTo(State.DecisionGenerated(creditApplication.State, input.Decision, DateTimeOffset.Now));

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private record Input
    {
        public string ApplicationId { get; init; }

        public Decision Decision { get; init; }
    }
}
