using Camunda.Orchestration.Sdk;
using Camunda.Client.Extensions;

namespace Camunda.Startup.DemoApp.Feature;

public class SendNotificationJobHandler : IJobHandler
{
    public Task HandleAsync(ActivatedJob job, CancellationToken ct)
    {
        var input = job.GetVariables<SendNotificationInput>();

        // TODO: Implement notification logic

        return Task.CompletedTask;
    }
}

public record SendNotificationInput(/* Add expected properties from process variables */);
