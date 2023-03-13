namespace Camunda.Connector.SDK.Core.Impl.Inbound;

public record InboundConnectorConfiguration
(
   string Name,
   string Type,
   Type ConnectorClass
);
