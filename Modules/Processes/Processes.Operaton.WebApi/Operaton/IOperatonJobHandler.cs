namespace Processes.Operaton.WebApi.Operaton;

public interface IOperatonJobHandler
{
    string Topic { get; }

    TimeSpan LockDuration => TimeSpan.FromSeconds(30);

    Task HandleAsync(OperatonExternalTask task, CancellationToken cancellationToken);
}
