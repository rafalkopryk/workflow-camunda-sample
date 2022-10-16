using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Common.Application.Serializer;
using Common.Application.Zeebe;
using MediatR;
using System.Text.Json;

namespace Applications.Application.UseCases.CloseApplication;

internal class CloseApplicationCommandHandler : IRequestHandler<CloseApplicationCommand>
{
    private readonly IZeebeService _zeebeService;
    private readonly CreditApplicationDbContext _creditApplicationDbContext;

    public CloseApplicationCommandHandler(IZeebeService zeebeService, CreditApplicationDbContext creditApplicationDbContext)
    {
        _zeebeService = zeebeService;
        _creditApplicationDbContext = creditApplicationDbContext;
    }

    public async Task<Unit> Handle(CloseApplicationCommand command, CancellationToken cancellationToken)
    {
        var input = JsonSerializer.Deserialize<Input>(command.Job.Variables, JsonSerializerCustomOptions.CamelCase);

        var creditApplication = await _creditApplicationDbContext.Applications.FindAsync(input.ApplicationId);
        creditApplication.ForwardTo(State.ApplicationClosed(creditApplication.State, DateTimeOffset.Now));

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private record Input
    {
        public Guid ApplicationId { get; init; }
    }
}
