namespace Common.Zeebe;

public interface IZeebeService
{
    Task<long> StartProcessInstance(string bpmProcessId, object veriables);
    Task PublishMessage(string messageName, string correlationKey, CancellationToken cancellationToken);
    Task SetVeriables(long elementInstanceKey, object variables, CancellationToken cancellationToken);
    Task CompleteJob(IJob job, CancellationToken cancellationToken);
}