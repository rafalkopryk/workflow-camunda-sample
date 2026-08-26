using Common.Application.Extensions;
using Common.Application.Cqrs;
using JasperFx.Resources;
using Processes.Operaton.WebApi.Features.CreditApplications.Close;
using Processes.Operaton.WebApi.Features.CreditApplications.CustomerVerification;
using Processes.Operaton.WebApi.Features.CreditApplications.Decision;
using Processes.Operaton.WebApi.Features.CreditApplications.Simulation;
using Processes.Operaton.WebApi.Operaton;
using Wolverine;
using Wolverine.AzureServiceBus;
using Wolverine.Kafka;

namespace Processes.Operaton.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddOperaton(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterHandlersFromAssemblies(typeof(ServiceCollectionExtensions).Assembly);

        services.AddHttpClient(OperatonClient.HttpClientName, client =>
        {
            var configuredAddress = configuration["OPERATON_REST_ADDRESS"]
                ?? configuration["Operaton:RestAddress"]
                ?? throw new InvalidOperationException("Operaton REST address is not configured.");
            var engineRestAddress = configuredAddress.Contains("/engine-rest", StringComparison.OrdinalIgnoreCase)
                ? configuredAddress
                : $"{configuredAddress.TrimEnd('/')}/engine-rest/";
            client.BaseAddress = new Uri(engineRestAddress.EndsWith('/') ? engineRestAddress : $"{engineRestAddress}/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddTransient<OperatonClient>();

        services.AddScoped<IOperatonJobHandler, SimulationJobHandler>();
        services.AddScoped<IOperatonJobHandler, DecisionJobHandler>();
        services.AddScoped<IOperatonJobHandler, CustomerVerificationJobHandler>();
        services.AddScoped<IOperatonJobHandler, CloseApplicationJobHandler>();

        services.AddHostedService<DeployBpmnService>();
        services.AddHostedService<OperatonWorkerService>();
    }

    public static void ConfigureWolverine(this WolverineOptions opts, IConfiguration configuration)
    {
        opts.UseRuntimeCompilation();

        if (configuration.IsKafka())
        {
            opts.UseKafka(configuration.GetkafkaConnectionString())
                .ConfigureConsumers(consumer => consumer = configuration.GetkafkaConsumer()!)
                .ConfigureProducers(producer => producer = configuration.GetkafkaProducer()!);

            opts.PublishMessage<CloseApplicationCommand>().ToKafkaTopic("applications").TelemetryEnabled(true);
            opts.PublishMessage<SimulationCommand>().ToKafkaTopic("simulations").TelemetryEnabled(true);
            opts.PublishMessage<DecisionCommand>().ToKafkaTopic("decisions").TelemetryEnabled(true);
            opts.PublishMessage<CustomerVerificationCommand>().ToKafkaTopic("customer-verifications").TelemetryEnabled(true);

            opts.ListenToKafkaTopic("applications").ProcessInline().TelemetryEnabled(true);
            opts.ListenToKafkaTopic("simulations").ProcessInline().TelemetryEnabled(true);
            opts.ListenToKafkaTopic("contracts").ProcessInline().TelemetryEnabled(true);
            opts.ListenToKafkaTopic("decisions").ProcessInline().TelemetryEnabled(true);
            opts.ListenToKafkaTopic("customer-verifications").ProcessInline().TelemetryEnabled(true);

            opts.Services.AddResourceSetupOnStartup();
        }
        else
        {
            opts.UseAzureServiceBus(configuration.GetAzServiceBusConnectionString()).AutoProvision();

            opts.PublishMessage<CloseApplicationCommand>().ToAzureServiceBusTopic("applications").TelemetryEnabled(true);
            opts.PublishMessage<SimulationCommand>().ToAzureServiceBusTopic("simulations").TelemetryEnabled(true);
            opts.PublishMessage<DecisionCommand>().ToAzureServiceBusTopic("decisions").TelemetryEnabled(true);
            opts.PublishMessage<CustomerVerificationCommand>().ToAzureServiceBusTopic("customer-verifications").TelemetryEnabled(true);

            opts.ListenToAzureServiceBusSubscription("applications-processes-subs")
                .FromTopic("applications").ProcessInline().TelemetryEnabled(true);
            opts.ListenToAzureServiceBusSubscription("simulations-processes-subs")
                .FromTopic("simulations").ProcessInline().TelemetryEnabled(true);
            opts.ListenToAzureServiceBusSubscription("contracts-processes-subs")
                .FromTopic("contracts").ProcessInline().TelemetryEnabled(true);
            opts.ListenToAzureServiceBusSubscription("decisions-processes-subs")
                .FromTopic("decisions").ProcessInline().TelemetryEnabled(true);
            opts.ListenToAzureServiceBusSubscription("customer-verifications-processes-subs")
                .FromTopic("customer-verifications").ProcessInline().TelemetryEnabled(true);
        }

        opts.Discovery.IncludeAssembly(typeof(ServiceCollectionExtensions).Assembly);
    }
}
