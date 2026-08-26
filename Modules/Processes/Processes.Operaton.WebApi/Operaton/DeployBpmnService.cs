namespace Processes.Operaton.WebApi.Operaton;

public sealed record ProcessDefinition(string Name, string Path);

internal sealed class DeployBpmnService(
    OperatonClient client,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<DeployBpmnService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var definitions = configuration.GetSection("ProcessDefinitions").Get<ProcessDefinition[]>() ?? [];

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (var definition in definitions)
                {
                    var path = Path.IsPathRooted(definition.Path)
                        ? definition.Path
                        : Path.Combine(environment.ContentRootPath, definition.Path);
                    await client.DeployAsync(
                        definition.Name,
                        await File.ReadAllBytesAsync(path, stoppingToken),
                        stoppingToken);
                    logger.LogInformation("Deployed Operaton process definition {Definition}", definition.Name);
                }

                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Could not deploy Operaton process definitions");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
