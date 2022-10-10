using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Impl.Builder;

namespace Common.Application.Zeebe;
internal class ZeebeWorker : IHostedService, IDisposable
{
    private CancellationTokenSource _cancellationTokenSource;
    private readonly List<IJobWorker> _workers = new();
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;

    public ZeebeWorker(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var address = _configuration.GetSection("ZEEBE:ADDRESS").Value;
        var clientId = _configuration.GetSection("ZEEBE:CLIENT:ID").Value;
        var clientSecret = _configuration.GetSection("ZEEBE:CLIENT:SECRET").Value;

        var client = CamundaCloudClientBuilder
            .Builder()
            .UseClientId(clientId)
            .UseClientSecret(clientSecret)
            .UseContactPoint(address)
            .Build();

        using var scope = _serviceScopeFactory.CreateScope();
        var zeebeJobHandlerProvider = scope.ServiceProvider.GetRequiredService<IZeebeJobHandlerProvider>();

        foreach (var jobHandlerInfo in zeebeJobHandlerProvider.GetJobs())
        {
            var zeebeJob = jobHandlerInfo.Type.GetZeebeJobAttribute();
            var worker = client.NewWorker()
                .JobType(zeebeJob.JobType)
                .Handler((jobClient, job) => HandleJob(job, jobHandlerInfo.Type, _cancellationTokenSource.Token))
                //.FetchVariables(zeebeJob.FetchVariabeles)
                .MaxJobsActive(zeebeJob.MaxJobsActive ?? 5)
                .Name(zeebeJob.JobType)
                .PollingTimeout(zeebeJob.PollingTimeout ?? TimeSpan.FromSeconds(60))
                .PollInterval(zeebeJob.PollInterval ?? TimeSpan.FromSeconds(10))
                .Timeout(zeebeJob.Timeout ?? TimeSpan.FromSeconds(60))
                .Open();

            _workers.Add(worker);
        }
    }

    private async Task HandleJob(IJob job, Type type, CancellationToken cancellationToken)
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



