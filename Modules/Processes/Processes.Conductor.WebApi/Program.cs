using Conductor.Api;
using Conductor.Client.Extensions;
using Conductor.Client.Interfaces;
using Conductor.Executor;
using Processes.Conductor.WebApi;
using Processes.Conductor.WebApi.Extensions;
using Processes.Conductor.WebApi.Features.CreditApplications.Close;
using Processes.Conductor.WebApi.Features.CreditApplications.CustomerVerification;
using Processes.Conductor.WebApi.Features.CreditApplications.Decision;
using Processes.Conductor.WebApi.Features.CreditApplications.Shared;
using Processes.Conductor.WebApi.Features.CreditApplications.Simulation;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.UseWolverine(options => options.ConfigureWolverine(builder.Configuration));

var conductorAddress = builder.Configuration["CONDUCTOR_REST_ADDRESS"]
    ?? throw new InvalidOperationException("CONDUCTOR_REST_ADDRESS is not configured.");

var conductorConfiguration = new Conductor.Client.Configuration
{
    BasePath = $"{conductorAddress.TrimEnd('/')}/api",
};

builder.Services
    .AddConductorWorker(conductorConfiguration)
    .WithHostedService();

builder.Services.AddSingleton(serviceProvider => new MetadataResourceApi(
    serviceProvider.GetRequiredService<Conductor.Client.Configuration>()));
builder.Services.AddSingleton(serviceProvider => new WorkflowResourceApi(
    serviceProvider.GetRequiredService<Conductor.Client.Configuration>()));
builder.Services.AddSingleton(serviceProvider => new TaskResourceApi(
    serviceProvider.GetRequiredService<Conductor.Client.Configuration>()));
builder.Services.AddSingleton(serviceProvider => new WorkflowExecutor(
    serviceProvider.GetRequiredService<Conductor.Client.Configuration>()));
builder.Services.AddSingleton<ConductorWorkflowService>();
builder.Services.AddSingleton<IWorkflowTask, SimulationTaskWorker>();
builder.Services.AddSingleton<IWorkflowTask, CustomerVerificationTaskWorker>();
builder.Services.AddSingleton<IWorkflowTask, DecisionTaskWorker>();
builder.Services.AddSingleton<IWorkflowTask, CloseApplicationTaskWorker>();
builder.Services.AddHostedService<DeployConductorDefinitionsService>();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
