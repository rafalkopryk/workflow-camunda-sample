using Elsa;
using Processes.Elsa.WebApi;
using Elsa.Studio.Authentication.ElsaIdentity.BlazorServer.Extensions;
using Elsa.Studio.Authentication.ElsaIdentity.HttpMessageHandlers;
using Elsa.Studio.Authentication.ElsaIdentity.UI.Extensions;
using Elsa.Studio.Authentication.OpenIdConnect.BlazorServer.Extensions;
using Elsa.Studio.Authentication.OpenIdConnect.HttpMessageHandlers;
using Elsa.Studio.Branding;
using Elsa.Studio.Contracts;
using Elsa.Studio.Core.BlazorServer.Extensions;
using Elsa.Studio.Dashboard.Extensions;
using Elsa.Studio.Extensions;
using Elsa.Studio.Localization.BlazorServer.Extensions;
using Elsa.Studio.Localization.Models;
using Elsa.Studio.Login.BlazorServer.Extensions;
using Elsa.Studio.Login.Extensions;
using Elsa.Studio.Login.HttpMessageHandlers;
using Elsa.Studio.Models;
using Elsa.Studio.Shell.Extensions;
using Elsa.Studio.Translations;
using Elsa.Studio.Workflows.Extensions;
using Elsa.Studio.Workflows.Designer.Extensions;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Elsa.Extensions;
using Elsa.Http.Options;
using Elsa.Persistence.EFCore.Extensions;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Elsa.Workflows.Api;
using Microsoft.Extensions.Options;
using Processes.Elsa.WebApi.Extensions;
using Processes.Elsa.WebApi.Features.CreditApplications;
using Processes.Elsa.WebApi.Features.CreditApplications.Close;
using Processes.Elsa.WebApi.Features.CreditApplications.CustomerVerification;
using Processes.Elsa.WebApi.Features.CreditApplications.Decision;
using Processes.Elsa.WebApi.Features.CreditApplications.Simulation;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var configuration = builder.Configuration;
var services = builder.Services;
var persistenceConnectionString = configuration.GetConnectionString("Sqlite") ?? throw new InvalidOperationException("Connection string 'Sqlite' is missing.");
var useStudioServer = true;

builder.UseWolverine(options => options.ConfigureWolverine(configuration));

builder.WebHost.UseStaticWebAssets();
services.AddRazorPages();
services.AddCors(cors => cors.AddDefaultPolicy(policy => policy
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowAnyOrigin()
    .WithExposedHeaders("*")));
services.AddHealthChecks();

var identitySection = configuration.GetSection("Identity");
var identityTokenSection = identitySection.GetSection("Tokens");

EndpointSecurityOptions.DisableSecurity();

services.AddElsa(elsa =>
{
    elsa
        .UseIdentity(identity =>
        {
            identity.TokenOptions += options => identityTokenSection.Bind(options);
            identity.UseConfigurationBasedUserProvider(options => identitySection.Bind(options));
            identity.UseConfigurationBasedApplicationProvider(options => identitySection.Bind(options));
            identity.UseConfigurationBasedRoleProvider(options => identitySection.Bind(options));
        })
        .UseDefaultAuthentication()
        .UseWorkflows()
        .UseWorkflowManagement(management => management.UseEntityFrameworkCore(ef =>
        {
            ef.UseSqlite(persistenceConnectionString);
        }))
        .UseWorkflowRuntime(runtime => runtime.UseEntityFrameworkCore(ef =>
        {
            ef.UseSqlite(persistenceConnectionString);
        }))
        .UseWorkflowsApi()
        .UseHttp(http => http.ConfigureHttpOptions = options => configuration.GetSection("Http").Bind(options))
        .UseScheduling()
        .UseJavaScript()
        .UseCSharp()
        .UseLiquid()
        .AddWorkflow<CreditApplicationWorkflow>()
        .AddActivity<PublishSimulationActivity>()
        .AddActivity<PublishCustomerVerificationActivity>()
        .AddActivity<PublishDecisionActivity>()
        .AddActivity<PublishCloseApplicationActivity>();
});

services.AddControllers();

if (useStudioServer)
{
    services.AddServerSideBlazor(options =>
    {
        options.RootComponents.MaxJSRootComponents = 1000;
        options.RootComponents.RegisterCustomElsaStudioElements();
    });

    var authenticationHandler = ConfigureStudioAuthentication(services, configuration);
    var backendApiConfig = new BackendApiConfig
    {
        ConfigureBackendOptions = options => configuration.GetSection("Backend").Bind(options),
        ConfigureHttpClientBuilder = options => options.AuthenticationHandler = authenticationHandler
    };
    var localizationConfig = new LocalizationConfig
    {
        ConfigureLocalizationOptions = options => configuration.GetSection("Localization").Bind(options)
    };

    services.AddScoped<IBrandingProvider, StudioBrandingProvider>();
    services.AddCore().Replace(new(typeof(IBrandingProvider), typeof(StudioBrandingProvider), ServiceLifetime.Scoped));
    services.AddShell(options => configuration.GetSection("Shell").Bind(options));
    services.AddRemoteBackend(backendApiConfig);
    services.AddDashboardModule();
    services.AddWorkflowsModule();
    services.AddWorkflowsDesigner();
    services.AddLocalizationModule(localizationConfig);
    services.AddTranslations();
    services.AddSignalR(options => options.MaximumReceiveMessageSize = 5 * 1024 * 1000);
}
else
{
    services.AddScoped<IBrandingProvider, StudioBrandingProvider>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

app.UseHttpsRedirection();
app.UseCors();
app.MapHealthChecks("/health");
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings =
        {
            [".dat"] = "application/octet-stream"
        }
    }
});
app.UseRouting();

var apiEndpointOptions = app.Services.GetRequiredService<IOptions<ApiEndpointOptions>>().Value;
var routePrefix = apiEndpointOptions.RoutePrefix;

app.MapWorkflowsApi(routePrefix);
app.UseAuthentication();
app.UseAuthorization();
app.UseJsonSerializationErrorHandler();
app.UseWorkflows();
app.MapControllers();

if (app.Environment.IsDevelopment())
    app.UseSwaggerUI();

if (useStudioServer)
{
    app.UseElsaLocalization();
    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");
}
else
{
    app.UseBlazorFrameworkFiles();
    app.MapFallbackToPage("/_WasmHost");
}

app.Run();

static Type ConfigureStudioAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var authProvider = configuration["Authentication:Provider"];
    if (string.IsNullOrWhiteSpace(authProvider))
        authProvider = "ElsaIdentity";

    if (authProvider.Equals("ElsaIdentity", StringComparison.OrdinalIgnoreCase))
    {
        services.AddElsaIdentity();
        services.AddElsaIdentityUI();
        return typeof(ElsaIdentityAuthenticatingApiHttpMessageHandler);
    }

    if (authProvider.Equals("OpenIdConnect", StringComparison.OrdinalIgnoreCase))
    {
        services.AddOpenIdConnectAuth(options => configuration.GetSection("Authentication:OpenIdConnect").Bind(options));
        return typeof(OidcAuthenticatingApiHttpMessageHandler);
    }

    if (authProvider.Equals("ElsaLogin", StringComparison.OrdinalIgnoreCase))
    {
        services.AddLoginModule().UseElsaIdentity();
        return typeof(AuthenticatingApiHttpMessageHandler);
    }

    throw new InvalidOperationException($"Unsupported Authentication:Provider value '{authProvider}'.");
}
