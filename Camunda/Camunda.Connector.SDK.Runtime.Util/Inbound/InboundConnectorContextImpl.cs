using Camunda.Client;
using Camunda.Connector.SDK.Core.Api.Inbound;
using Camunda.Connector.SDK.Core.Impl.Inbound;
using Camunda.Connector.SDK.Runtime.Util.Inbound.Correlation;
using System.Text.Json;

namespace Camunda.Connector.SDK.Runtime.Util.Inbound;

public class InboundConnectorContextImpl : IInboundConnectorContext
{
    private readonly InboundConnectorProperties _properties;
    private readonly InboundCorrelationHandler _correlationHandler;

    public InboundConnectorContextImpl(InboundConnectorProperties properties, InboundCorrelationHandler correlationHandler)
    {
        _properties = properties;
        _correlationHandler = correlationHandler;
    }

    public async Task<InboundConnectorResult> Correlate(string variables)
    {
        return await _correlationHandler.Correlate(_properties.CorrelationPoint, variables);
    }

    public InboundConnectorProperties GetProperties()
    {
        return _properties;
    }

    public T GetPropertiesAsType<T>()
    {
        var properties = JsonSerializer.Serialize(_properties.Properties, JsonSerializerCustomOptions.CamelCase);
        return JsonSerializer.Deserialize<T>(properties, JsonSerializerCustomOptions.CamelCase);
    }

    public void ReplaceSecrets(object var1)
    {
        //TODO
    }

    public void Validate(object var1)
    {
        //TODO
    }
}
