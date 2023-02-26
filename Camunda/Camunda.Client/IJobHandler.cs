namespace Camunda.Client
{
    public interface IJobHandler
    {
        public Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken);
    }
}