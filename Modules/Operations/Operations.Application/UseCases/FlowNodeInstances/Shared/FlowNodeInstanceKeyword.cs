﻿internal static class FlowNodeInstanceKeyword
{
    public const string INDEX = "zeebe-record-process-instance";
    public const string INTENT_ELEMENT_ACTIVATED = "ELEMENT_ACTIVATED";
    public const string INTENT_ELEMENT_COMPLETED = "ELEMENT_COMPLETED";
    public const string INTENT_ELEMENT_TERMINATED = "ELEMENT_TERMINATED";

    public const string VALUETYPE = "PROCESS_INSTANCE";

    public const string BPMN_ELEMENT_TYPE_UNSPECIFIED = "UNSPECIFIED";
    public const string BPMN_ELEMENT_TYPE_PROCESS = "PROCESS";
    public const string BPMN_ELEMENT_TYPE_SUB_PROCESS = "SUB_PROCESS";
    public const string BPMN_ELEMENT_TYPE_EVENT_SUB_PROCESS = "EVENT_SUB_PROCESS";
    public const string BPMN_ELEMENT_TYPE_START_EVENT = "START_EVENT";
    public const string BPMN_ELEMENT_TYPE_INTERMEDIATE_CATCH_EVENT = "INTERMEDIATE_CATCH_EVENT";
    public const string BPMN_ELEMENT_TYPE_INTERMEDIATE_THROW_EVENT = "INTERMEDIATE_THROW_EVENT";
    public const string BPMN_ELEMENT_TYPE_BOUNDARY_EVENT = "BOUNDARY_EVENT";
    public const string BPMN_ELEMENT_TYPE_END_EVENT = "END_EVENT";
    public const string BPMN_ELEMENT_TYPE_SERVICE_TASK = "SERVICE_TASK";
    public const string BPMN_ELEMENT_TYPE_RECEIVE_TASK = "RECEIVE_TASK";
    public const string BPMN_ELEMENT_TYPE_USER_TASK = "USER_TASK";
    public const string BPMN_ELEMENT_TYPE_MANUAL_TASK = "MANUAL_TASK";
    public const string BPMN_ELEMENT_TYPE_TASK = "TASK";
    public const string BPMN_ELEMENT_TYPE_EXCLUSIVE_GATEWAY = "EXCLUSIVE_GATEWAY";
    public const string BPMN_ELEMENT_TYPE_INCLUSIVE_GATEWAY = "INCLUSIVE_GATEWAY";
    public const string BPMN_ELEMENT_TYPE_PARALLEL_GATEWAY = "PARALLEL_GATEWAY";
    public const string BPMN_ELEMENT_TYPE_EVENT_BASED_GATEWAY = "EVENT_BASED_GATEWAY";
    public const string BPMN_ELEMENT_TYPE_SEQUENCE_FLOW = "SEQUENCE_FLOW";
    public const string BPMN_ELEMENT_TYPE_MULTI_INSTANCE_BODY = "MULTI_INSTANCE_BODY";
    public const string BPMN_ELEMENT_TYPE_CALL_ACTIVITY = "CALL_ACTIVITY";
    public const string BPMN_ELEMENT_TYPE_BUSINESS_RULE_TASK = "BUSINESS_RULE_TASK";
    public const string BPMN_ELEMENT_TYPE_SCRIPT_TASK = "SCRIPT_TASK";
    public const string BPMN_ELEMENT_TYPE_SEND_TASK = "SEND_TASK";
    public const string BPMN_ELEMENT_TYPE_UNKNOWN = "UNKNOWN";

    public static readonly string[] FLOW_NODE_BPMN_ELEMENT_TYPES = new string[]
    {
        BPMN_ELEMENT_TYPE_PROCESS, BPMN_ELEMENT_TYPE_SUB_PROCESS, BPMN_ELEMENT_TYPE_EVENT_SUB_PROCESS,
        BPMN_ELEMENT_TYPE_START_EVENT, BPMN_ELEMENT_TYPE_INTERMEDIATE_CATCH_EVENT, BPMN_ELEMENT_TYPE_INTERMEDIATE_THROW_EVENT, BPMN_ELEMENT_TYPE_BOUNDARY_EVENT, BPMN_ELEMENT_TYPE_END_EVENT,
        BPMN_ELEMENT_TYPE_SERVICE_TASK, BPMN_ELEMENT_TYPE_RECEIVE_TASK, BPMN_ELEMENT_TYPE_USER_TASK, BPMN_ELEMENT_TYPE_MANUAL_TASK, BPMN_ELEMENT_TYPE_TASK,
        BPMN_ELEMENT_TYPE_EXCLUSIVE_GATEWAY, BPMN_ELEMENT_TYPE_INCLUSIVE_GATEWAY, BPMN_ELEMENT_TYPE_PARALLEL_GATEWAY, BPMN_ELEMENT_TYPE_EVENT_BASED_GATEWAY,
        BPMN_ELEMENT_TYPE_MULTI_INSTANCE_BODY, BPMN_ELEMENT_TYPE_CALL_ACTIVITY, BPMN_ELEMENT_TYPE_BUSINESS_RULE_TASK, BPMN_ELEMENT_TYPE_SCRIPT_TASK, BPMN_ELEMENT_TYPE_SEND_TASK
    };
}