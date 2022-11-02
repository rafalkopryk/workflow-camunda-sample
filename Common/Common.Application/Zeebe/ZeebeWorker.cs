using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Builder;

namespace Common.Application.Zeebe;

internal class ZeebeWorker : IHostedService, IDisposable
{
    private CancellationTokenSource _cancellationTokenSource;
    private readonly List<IJobWorker> _workers = new();
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ZeebeOptions _zeebeOptions;

    public ZeebeWorker(IServiceScopeFactory serviceScopeFactory, IOptions<ZeebeOptions> zeebeOptions)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _zeebeOptions = zeebeOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var client = _zeebeOptions.Cloud
            ? CamundaCloudClientBuilder
                .Builder()
                .UseClientId(_zeebeOptions.Client.Id)
                .UseClientSecret(_zeebeOptions.Client.Secret)
                .UseContactPoint(_zeebeOptions.Address)
                .Build()
            : ZeebeClient.Builder()
                .UseGatewayAddress(_zeebeOptions.Address)
                .UsePlainText()
                .UseKeepAlive(TimeSpan.FromSeconds(60))
                .Build();

        using var scope = _serviceScopeFactory.CreateScope();
        var zeebeJobHandlerProvider = scope.ServiceProvider.GetRequiredService<IZeebeJobHandlerProvider>();

        foreach (var jobHandlerInfo in zeebeJobHandlerProvider.GetJobs())
        {
            var zeebeJob = jobHandlerInfo.Type.GetZeebeJobAttribute();
            var worker = client.NewWorker()
                .JobType(zeebeJob.JobType)
                .Handler((jobClient, job) => HandleJob(jobClient, job, jobHandlerInfo.Type, _cancellationTokenSource.Token))
                //.FetchVariables(zeebeJob.FetchVariabeles)
                .MaxJobsActive(zeebeJob.MaxJobsActive)
                .Name(zeebeJob.JobType)
                .PollingTimeout(TimeSpan.FromMilliseconds(zeebeJob.PollingTimeoutInMs))
                .PollInterval(TimeSpan.FromMilliseconds(zeebeJob.PollIntervalInMs))
                .Timeout(TimeSpan.FromMilliseconds(zeebeJob.TimeoutInMs))
                .Open();

            _workers.Add(worker);
        }
    }

    private async Task HandleJob(IJobClient jobClient, IJob job, Type type, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            await Task.FromCanceled(cancellationToken);

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var instance = Activator.CreateInstance(type);
            ((IZeebeJob)instance).Job = job;

            await mediator.Send(instance, cancellationToken);

            await jobClient
                .NewCompleteJobCommand(job)
                .Send(cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _cancellationTokenSource.Cancel();
        }
        finally
        {
            StopInternal();
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        StopInternal();
    }

    public void StopInternal()
    {
        _workers.ForEach(w => w.Dispose());
        _workers.Clear();
    }
}



