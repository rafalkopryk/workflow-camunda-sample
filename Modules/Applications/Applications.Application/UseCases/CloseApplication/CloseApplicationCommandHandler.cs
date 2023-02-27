using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Camunda.Client;
using Common.Application.Serializer;
using System.Text.Json;

namespace Applications.Application.UseCases.CloseApplication;

[ZeebeWorker(Type = "close-application", MaxJobsToActivate = 10, PollingTimeoutInMs = 15_000, PollIntervalInMs = 500, RetryBackOffInMs = new[] { 5_000, 15_000 })]
internal class CloseApplicationCommandHandler : IJobHandler
{
    private readonly CreditApplicationDbContext _creditApplicationDbContext;

    public CloseApplicationCommandHandler(CreditApplicationDbContext creditApplicationDbContext)
    {
        _creditApplicationDbContext = creditApplicationDbContext;
    }

    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        var input = JsonSerializer.Deserialize<Input>(job.Variables, Camunda.Client.JsonSerializerCustomOptions.CamelCase);

        var creditApplication = await _creditApplicationDbContext.Applications.FindAsync(input.ApplicationId);
        creditApplication.ForwardTo(State.ApplicationClosed(creditApplication.State, DateTimeOffset.Now));

        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);
    }

    private record Input
    {
        public string ApplicationId { get; init; }
    }
}
