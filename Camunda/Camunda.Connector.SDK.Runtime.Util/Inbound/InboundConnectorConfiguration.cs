using Camunda.Connector.SDK.Core.Api.Inbound;
using Microsoft.Extensions.DependencyInjection;

namespace Camunda.Connector.SDK.Runtime.Util.Inbound;

public class DefaultInboundConnectorFactory : IInboundConnectorFactoryConnectorFactory
{
    private readonly IEnumerable<InboundConnectorConfiguration> _inboundConnectorConfigurations;

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DefaultInboundConnectorFactory(IEnumerable<InboundConnectorConfiguration> inboundConnectorConfigurations, IServiceScopeFactory serviceScopeFactory)
    {
        _inboundConnectorConfigurations = inboundConnectorConfigurations;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public List<InboundConnectorConfiguration> GetConfigurations()
    {
        return _inboundConnectorConfigurations.ToList();
    }

    public IInboundConnectorExecutable GetInstance(string type)
    {
        var inboundConnectorConfiguration = _inboundConnectorConfigurations.FirstOrDefault(x => x.Type == type);
        using var scope = _serviceScopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService(inboundConnectorConfiguration.ConnectorClass);
        return service as IInboundConnectorExecutable;
    }

    public void RegisterConfiguration(InboundConnectorConfiguration configuration)
    {
        //TODO
    }

    public void ResetConfigurations()
    {
        //TODO
    }
}