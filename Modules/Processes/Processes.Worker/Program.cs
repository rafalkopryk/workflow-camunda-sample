using OpenTelemetry.Resources;
using Common.Application.Extensions;
using Processes.Application.Extensions;
using Wolverine;

var host = Host.CreateDefaultBuilder(args)
    .UseWolverine((ctx, opts) => opts.ConfigureWolverine(ctx.Configuration))
    .ConfigureServices((ctx, services)  => 
    {
        services.Configure<HostOptions>(options =>
        {
            options.ServicesStartConcurrently = true;
            options.ServicesStopConcurrently = false;
        });

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService("Credit.Processes", serviceVersion: "1.0.0")
            .AddTelemetrySdk();

        services.AddInfrastructure(ctx.Configuration, resourceBuilder);
        services.AddApplication(ctx.Configuration);
    }).Build();

await host.RunAsync();