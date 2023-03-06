namespace Camunda.Connector.SDK.Runtime.Util.Inbound;

public record InboundConnectorConfiguration
(
   string Name,
   string Type,
   Type ConnectorClass
);
