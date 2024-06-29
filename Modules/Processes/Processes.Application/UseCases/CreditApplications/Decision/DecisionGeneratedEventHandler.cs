using Camunda.Client;
using MassTransit;
using MediatR;

namespace Processes.Application.UseCases.CreditApplications.Decision;

[ZeebeMessage(Name = "Message_DecisionGenerated")]
[EntityName("event.credit.applications.decisionGenerated.v1")]
[MessageUrn("event.credit.applications.decisionGenerated.v1")]
public record DecisionGenerated(string ApplicationId);

internal class DecisionGeneratedEventHandler(IMessageClient messageClient) : IConsumer<DecisionGenerated>
{
    private readonly IMessageClient _client = messageClient;

    public async Task Consume(ConsumeContext<DecisionGenerated> context)
    {
        await _client.Publish(context.Message.ApplicationId, context.Message);
    }
}
