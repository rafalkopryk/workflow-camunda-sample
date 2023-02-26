using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Camunda.Client;
using Common.Application.Dictionary;
using Common.Application.Serializer;
using MediatR;
using System.Text.Json;

namespace Applications.Application.UseCases.SetDecision;

[ZeebeWorker(Type = "set-decision-data", MaxJobsToActivate = 10, PollingTimeoutInMs = 15_000, PollIntervalInMs = 500, RetryBackOffInMs = new[] { 1_000, 5_000 })]
internal class SetDecisionCommandCommandHandler : IJobHandler
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext;

    public SetDecisionCommandCommandHandler(CreditApplicationDbContext creditApplicationDbContext)
    {
        _creditApplicationDbContext = creditApplicationDbContext;
    }

    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {

        var input = JsonSerializer.Deserialize<Input>(job.Variables, JsonSerializerCustomOptions.CamelCase);

        var creditApplication = await _creditApplicationDbContext.Applications.FindAsync(input.ApplicationId);
        creditApplication.ForwardTo(State.DecisionGenerated(creditApplication.State, input.Decision, DateTimeOffset.Now));

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);
    }

    private record Input
    {
        public string ApplicationId { get; init; }

        public Decision Decision { get; init; }
    }
}
