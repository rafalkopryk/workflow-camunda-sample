namespace Operations.BackOffice.Client.Data.ProcessFlowNodeInstances.Dto;

public record FlowNodeInstanceDto
{
    public long Key { get; init; }

    public long ProcessInstanceKey { get; init; }

    public long ProcessDefinitionKey { get; init; }

    public DateTimeOffset StartDate { get; init; }

    public DateTimeOffset? EndDate { get; set; }

    public string FlowNodeId { get; init; }

    public string FlowNodeName { get; set; }

    public long IncidentKey { get; init; }

    public FlowNodeInstanceType? Type { get; init; }
    public FlowNodeInstanceState State { get; init; }

    //public bool Incident { get; set; }
}


public enum FlowNodeInstanceType
{
    UNSPECIFIED = 0,
    PROCESS = 1,
    SUB_PROCESS = 2,
    EVENT_SUB_PROCESS = 3,
    START_EVENT = 4,
    INTERMEDIATE_CATCH_EVENT = 5,
    INTERMEDIATE_THROW_EVENT = 6,
    BOUNDARY_EVENT = 7,
    END_EVENT = 8,
    SERVICE_TASK = 9,
    RECEIVE_TASK = 10,
    USER_TASK = 11,
    MANUAL_TASK = 12,
    TASK = 13,
    EXCLUSIVE_GATEWAY = 14,
    INCLUSIVE_GATEWAY = 15,
    PARALLEL_GATEWAY = 16,
    EVENT_BASED_GATEWAY = 17,
    SEQUENCE_FLOW = 18,
    MULTI_INSTANCE_BODY = 19,
    CALL_ACTIVITY = 20,
    BUSINESS_RULE_TASK = 21,
    SCRIPT_TASK = 22,
    SEND_TASK = 23,
    UNKNOWN = 24,
}

public enum FlowNodeInstanceState
{
    ACTIVE = 0,
    COMPLETED = 1,
    TERMINATED = 2,
}