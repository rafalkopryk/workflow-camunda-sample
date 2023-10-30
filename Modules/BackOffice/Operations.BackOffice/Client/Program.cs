using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Operations.BackOffice.Client;
using Operations.BackOffice.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped<IProcessInstanceService, ProcessInstanceService>();
builder.Services.AddScoped<IProcessDefinitionService, ProcessDefinitionService>();

builder.Services.Configure<ExternalServicesOptions>(options => builder.Configuration.GetSection("externalServices").Bind(options));


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMudServices();

//await JSHost.ImportAsync("Interop", "../Pages/BPMN.razor.js");

await builder.Build().RunAsync();
