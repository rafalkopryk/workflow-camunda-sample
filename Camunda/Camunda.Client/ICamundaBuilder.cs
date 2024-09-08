using Camunda.Client.Jobs;

namespace Camunda.Client;

public interface ICamundaBuilder
{
    ICamundaBuilder AddWorker<T>(JobWorkerConfiguration jobWorkerConfiguration) where T : class, IJobHandler;

    ICamundaBuilder AddWorker<T>(string type, JobWorkerConfiguration jobWorkerConfiguration, string[]? fetchVariabeles = null) where T : class, IJobHandler;
}
