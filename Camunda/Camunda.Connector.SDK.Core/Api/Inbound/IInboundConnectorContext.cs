using Camunda.Connector.SDK.Core.Impl.Inbound;

namespace Camunda.Connector.SDK.Core.Api.Inbound;

public interface IInboundConnectorContext
{
    void ReplaceSecrets(object var1);

    void Validate(object var1);

    Task<IInboundConnectorResult<IResponseData>> Correlate(object variables);

    void Cancel();

    InboundConnectorProperties GetProperties();

    T GetPropertiesAsType<T>();
}
