using Conductor.Api;
using Conductor.Executor;
using Processes.Conductor.WebApi.ConductorDefinitions;

namespace Processes.Conductor.WebApi;

internal sealed class DeployConductorDefinitionsService(
    MetadataResourceApi metadataApi,
    WorkflowExecutor workflowExecutor,
    ILogger<DeployConductorDefinitionsService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var taskDefinitions = CreditApplicationTaskDefinitions.Create();
                var workflowDefinition = CreditApplicationWorkflowDefinition.Create();

                var existingTaskNames = (await metadataApi.GetTaskDefsAsync())
                    .Select(task => task.Name)
                    .ToHashSet(StringComparer.Ordinal);

                var newTaskDefinitions = taskDefinitions
                    .Where(task => !existingTaskNames.Contains(task.Name))
                    .ToList();
                if (newTaskDefinitions.Count > 0)
                {
                    await metadataApi.RegisterTaskDefAsync(newTaskDefinitions);
                }

                foreach (var taskDefinition in taskDefinitions.Where(task => existingTaskNames.Contains(task.Name)))
                {
                    await metadataApi.UpdateTaskDefAsync(taskDefinition);
                }

                workflowExecutor.RegisterWorkflow(workflowDefinition, overwrite: true);

                logger.LogInformation(
                    "Registered Conductor workflow {WorkflowName} version {WorkflowVersion}",
                    workflowDefinition.Name,
                    workflowDefinition.Version);
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to register Conductor definitions; retrying in 5 seconds");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
