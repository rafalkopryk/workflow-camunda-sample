using Camunda.Client;

namespace Utility.KafkaConnector.UseCasaes;

[ZeebeWorker(Type = "Error")]
internal class ThrowErrorTaskHandler : IJobHandler
{
    public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}