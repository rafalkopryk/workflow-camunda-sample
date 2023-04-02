using Camunda.Connector.SDK.Core.Impl.Inbound.Correlation;
using System.Text.Json.Serialization;

namespace Camunda.Connector.SDK.Core.Impl.Inbound;

public abstract class ProcessCorrelationPoint
{
    public ProcessCorrelationPoint()
    {
    }

    public abstract string GetId();
}
