# Camunda.Client

## Installation
To integrate Zeebe with your .NET application, you need to install the necessary packages. Add the following packages to your project:

    //TODO

## Configuration
You can configure the Zeebe integration using either attributes or parameters.

### Attribute Configuration
To configure the Zeebe client using attributes, add the following code to your Startup.cs or equivalent file:

    services.AddZeebe(
        options => configuration.GetSection("Zeebe").Bind(options),
        builder => builder
            .AddWorker<Task1JobHandler>());

### Parameter Configuration
To configure the Zeebe client using parameters, add the following code to your Startup.cs or equivalent file:

    services.AddZeebe(
        options => configuration.GetSection("Zeebe").Bind(options),
        builder => builder
            .AddWorker<Task1JobHandler>(new ServiceTaskConfiguration
            {
                Type = "task1",
                AutoComplete = true,
            }));

### Job Worker Implementation
Below is an example implementation of a job worker using the attribute configuration:

    [JobWorker(Type = "task1")]
    internal class Task1JobHandler : IJobHandler
    {
        public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
        {
          // Your logic here...
        }
    }

### Configuration Fields
The JobWorkerAttribute and ServiceTaskConfiguration has several fields that can be configured:

    public string Type { get; init; }
    public long TimeoutInMs { get; init; } = 60_000;
    public string[] FetchVariabeles { get; init; } = [];
    public int[] RetryBackOffInMs { get; init; } = [1000];
    public bool AutoComplete { get; init; } = true;
    public string[] TenatIds { get; init; } = [];
    public int PoolingMaxJobsToActivate { get; init; } = 20;
    public long PoolingRequestTimeoutInMs { get; set; } = 20_000;
    public int PoolingDelayInMs { get; init; } = 100;
    public bool UseStream { get; init; }
    public int StreamTimeoutInSec { get; init; } = 900;

Description of Fields
- Type: the job type. Used as reference to specify which job workers request the respective camunda task.
- TimeoutInMs: a job returned after this call will not be activated by another call until the timeout (in ms) has been reached.
- FetchVariabeles: a list of variables to fetch as the job variables; if empty, all visible variables at the time of activation for the scope of the job will be returned.
- RetryBackOffInMs: Backoff intervals for retries in milliseconds.
- AutoComplete: Indicates if the job should be auto-completed.
- TenatIds: Tenant IDs.
- PoolingMaxJobsToActivate: Maximum jobs to activate in one polling.
- PoolingRequestTimeoutInMs: Timeout for the polling request in milliseconds. The request will be completed when at least one job is activated or after the requestTimeout (in ms). 
  - if the requestTimeout = 0, a default timeout is used. 
  - if the requestTimeout < 0, long polling is disabled and the request is completed immediately, even when no job is activated.
- PoolingDelayInMs: Delay between polling attempts in milliseconds.
- UseStream: Indicates if streaming should be used.
- StreamTimeoutInSec: Timeout for the stream in seconds.

WARMING!!!
Even with streaming enabled, job workers still poll the cluster for jobs. Due to implementation constraints https://docs.camunda.io/docs/apis-tools/java-client/job-worker/#backfilling.

For UseStream = true recomends set pooling fields:
- PoolingRequestTimeoutInMs: -1
- PoolingDelayInMs: 10_000

### Auto-completing jobs
By default, the autoComplete attribute is set to true for any job worker.

In this case, the integration will handle job completion for you:

    [JobWorker(Type = "task1", AutoComplete = true)]
    internal class Task1JobHandler : IJobHandler
    {
      public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
      {
        // Your logic here...
      }
    }

### Programmatically completing jobs
Your job worker code can also complete the job itself.

    [JobWorker(Type = "task1", AutoComplete = false)]
    internal class Task1JobHandler : IJobHandler
    {
      public async Task Handle(IJobClient client, IJob job, CancellationToken cancellationToken)
      {
        // Your logic here...

        await client.CompleteJobCommand(
            job,
            """
            {
                "message": "task1 message"    
            }
            """);
      }
    }