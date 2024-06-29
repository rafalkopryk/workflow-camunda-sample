using Camunda.Client;
using MassTransit;

namespace Processes.Application.UseCases.CreditApplications.Close;

[ZeebeMessage(Name = "Message_ApplicationClosed")]
[EntityName("event.credit.applications.applicationClosed.v1")]
[MessageUrn("event.credit.applications.applicationClosed.v1")]
public record ApplicationClosed(string ApplicationId);

internal class ApplicationClosedEventHandler(IMessageClient messageClient) : IConsumer<ApplicationClosed>
{
    private readonly IMessageClient _client = messageClient;

    public async Task Consume(ConsumeContext<ApplicationClosed> context)
    {
        await _client.Publish(context.Message.ApplicationId, context.Message);
    }
}
