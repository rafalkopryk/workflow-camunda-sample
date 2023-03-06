using Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;
using System.Text.Json.Serialization;

namespace Camunda.Connector.SDK.Core.Api.Inbound;

[JsonDerivedType(typeof(MessageCorrelationPoint), typeDiscriminator: "MessageCorrelationPoint")]
[JsonDerivedType(typeof(StartEventCorrelationPoint), typeDiscriminator: "StartEventCorrelationPoint")]
public abstract class ProcessCorrelationPoint
{
    public ProcessCorrelationPoint()
    {
    }

    public abstract string GetId();
}
