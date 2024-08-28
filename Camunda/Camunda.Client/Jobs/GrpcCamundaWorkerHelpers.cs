using GatewayProtocol;

namespace Camunda.Client.Workers
{
    internal static class GrpcCamundaWorkerHelpers
    {
        internal static IJob Map(ActivatedJob activatedJob)
        {
            return new InternalJob
            {
                BpmnProcessId = activatedJob.BpmnProcessId,
                CustomHeaders = activatedJob.CustomHeaders,
                Deadline = DateTimeOffset.FromUnixTimeMilliseconds(activatedJob.Deadline).ToLocalTime(),
                Variables = activatedJob.Variables,
                ElementId = activatedJob.ElementId,
                ElementInstanceKey = activatedJob.ElementInstanceKey,
                Key = activatedJob.Key,
                ProcessDefinitionKey = activatedJob.ProcessDefinitionKey,
                ProcessDefinitionVersion = activatedJob.ProcessDefinitionVersion,
                ProcessInstanceKey = activatedJob.ProcessInstanceKey,
                Retries = activatedJob.Retries,
                Type = activatedJob.Type,
                Worker = activatedJob.Worker,
            };
        }
    }
}