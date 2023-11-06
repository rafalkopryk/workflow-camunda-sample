using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Operations.BackOffice.Client;
using Operations.BackOffice.Client.Components.BPMNViewer;
using Operations.BackOffice.Client.Data.Incidents;
using Operations.BackOffice.Client.Data.ProcessDefinitions;
using Operations.BackOffice.Client.Data.ProcessInstances;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.RootComponents.RegisterCustomElement<BPMNViewer>("bpmn-viewer");


builder.Services.AddScoped<IProcessInstanceService, ProcessInstanceService>();
builder.Services.AddScoped<IProcessDefinitionService, ProcessDefinitionService>();
builder.Services.AddScoped<IProcessIncidentService, ProcessIncidentService>();

builder.Services.Configure<ExternalServicesOptions>(options => builder.Configuration.GetSection("externalServices").Bind(options));


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMudServices();

await builder.Build().RunAsync();
