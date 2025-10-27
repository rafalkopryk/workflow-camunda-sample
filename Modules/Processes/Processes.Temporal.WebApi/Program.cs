using Processes.Temporal.WebApi.Domain.CreditApplications;
using Processes.Temporal.WebApi.Extensions;
using Processes.Temporal.WebApi.UseCases.CreditApplications;
using Processes.Temporal.WebApi.UseCases.CreditApplications.Close;
using Processes.Temporal.WebApi.UseCases.CreditApplications.CustomerVerification;
using Processes.Temporal.WebApi.UseCases.CreditApplications.Decision;
using Processes.Temporal.WebApi.UseCases.CreditApplications.Simulation;
using Temporalio.Extensions.Hosting;
using Temporalio.Extensions.OpenTelemetry;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.UseWolverine(opts => opts.ConfigureWolverine(builder.Configuration));

builder.Services.AddOpenApi();


var temporalConnectionString = builder.Configuration.GetConnectionString("temporal");
builder.Services
    .AddTemporalClient(opts =>
    {
        opts.TargetHost = temporalConnectionString;
        opts.Namespace = "default";
        opts.Interceptors = [new TracingInterceptor()];
    })
    .AddHostedTemporalWorker(
        clientTargetHost: temporalConnectionString,
        clientNamespace: "default",
        taskQueue: CreditApplicationKeyword.CREDIT_APPLICATION_TASK_QUEUE)
    .ConfigureOptions(opts =>
    {
        opts.Interceptors = [new TracingInterceptor()];
        opts.AddWorkflow<CreditApplicationWorkflow>();
    })
    .AddScopedActivities<SimulationService>()
    .AddScopedActivities<DecisionService>()
    .AddScopedActivities<CustomerVerificationService>()
    .AddScopedActivities<CloseApplicationService>();

var app = builder.Build();
app.MapDefaultEndpoints();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
