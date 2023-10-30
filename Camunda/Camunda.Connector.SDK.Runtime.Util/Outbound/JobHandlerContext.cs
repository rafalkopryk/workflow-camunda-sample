using Camunda.Client;
using Camunda.Connector.SDK.Core.Api.Outbound;
using Camunda.Connector.SDK.Core.Impl.Context;
using System.Text.Json;

namespace Camunda.Connector.SDK.Runtime.Util.Outbound;

public class JobHandlerContext : AbstractConnectorContext, IOutboundConnectorContext
{
    private readonly IJob _job;

    public JobHandlerContext(IJob job) : base()
    {
        _job = job;
    }

    public string GetVariables()
    {
        return _job.Variables;
    }

    public T GetVariablesAsType<T>()
    {
        return _job.GetVariablesAsType<T>();
    }

    public void ReplaceSecrets(object input)
    {
        throw new NotImplementedException();
    }

    public void Validate(object input)
    {
        throw new NotImplementedException();
    }
}

