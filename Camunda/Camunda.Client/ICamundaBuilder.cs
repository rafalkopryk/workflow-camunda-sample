namespace Camunda.Client;

public interface ICamundaBuilder
{
    ICamundaBuilder AddWorker<T>() where T : class, IJobHandler;


    ICamundaBuilder AddWorker<T>(JobWorkerConfiguration jobWorkerConfiguration) where T : class, IJobHandler;
}
