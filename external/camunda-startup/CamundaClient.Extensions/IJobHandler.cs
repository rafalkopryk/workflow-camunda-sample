using Camunda.Orchestration.Sdk;

namespace Camunda.Client.Extensions;

public interface IJobHandler
{
    Task HandleAsync(ActivatedJob job, CancellationToken ct);
}

public interface IJobHandler<T>
{
    Task<T> HandleAsync(ActivatedJob job, CancellationToken ct);
}
