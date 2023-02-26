namespace Camunda.Connector.SDK.Core.Api.Outbound;

public interface IOutboundConnectorContext
{
    string GetVariables();

    T GetVariablesAsType<T>();

    void ReplaceSecrets(object input);

    void Validate(object input);
}
