using Microsoft.Extensions.DependencyInjection;

namespace Processes.Operaton.WebApi.Operaton;

internal sealed class OperatonWorkerService(
    OperatonClient client,
    IServiceScopeFactory scopeFactory,
    ILogger<OperatonWorkerService> logger) : BackgroundService
{
    private readonly string _workerId = $"{Environment.MachineName}-{Guid.NewGuid():N}";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var handlers = scope.ServiceProvider.GetServices<IOperatonJobHandler>().ToArray();
                var tasks = await client.FetchAndLockAsync(_workerId, handlers, stoppingToken);
                await Task.WhenAll(tasks.Select(task => ProcessAsync(task, handlers, stoppingToken)));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to fetch Operaton external tasks");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ProcessAsync(
        OperatonExternalTask task,
        IReadOnlyCollection<IOperatonJobHandler> handlers,
        CancellationToken cancellationToken)
    {
        var handler = handlers.SingleOrDefault(x => x.Topic == task.TopicName);
        if (handler is null)
        {
            logger.LogWarning("No handler is registered for Operaton topic {Topic}", task.TopicName);
            return;
        }

        try
        {
            await handler.HandleAsync(task, cancellationToken);
            await client.CompleteAsync(task.Id, _workerId, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Operaton task {TaskId} on topic {Topic} failed", task.Id, task.TopicName);
            try
            {
                await client.ReportFailureAsync(task, _workerId, exception, cancellationToken);
            }
            catch (Exception reportException)
            {
                logger.LogError(reportException, "Could not report failure for Operaton task {TaskId}", task.Id);
            }
        }
    }
}
