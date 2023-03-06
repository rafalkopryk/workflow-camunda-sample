using Camunda.Connector.SDK.Core.Impl.Inbound;

namespace Camunda.Connector.SDK.Core.Api.Inbound;

public interface IInboundConnectorContext
{
    void ReplaceSecrets(object var1);

    void Validate(object var1);

    Task<InboundConnectorResult> Correlate(string variables);

    InboundConnectorProperties GetProperties();

    T GetPropertiesAsType<T>();
}
