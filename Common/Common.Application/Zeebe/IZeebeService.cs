using Zeebe.Client.Api.Responses;

namespace Common.Application.Zeebe;

public interface IZeebeService
{
    Task<long> StartProcessInstance(string bpmProcessId, object veriables);

    Task PublishMessage(string messageName, string correlationKey, CancellationToken cancellationToken);

    Task SetVeriables(IJob job, object variables, CancellationToken cancellationToken);
}
