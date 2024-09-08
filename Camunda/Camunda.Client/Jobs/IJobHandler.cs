namespace Camunda.Client.Jobs
{
    public interface IJobHandler
    {
        public Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken);
    }
}