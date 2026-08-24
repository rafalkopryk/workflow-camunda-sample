namespace Processes.Conductor.WebApi.Features.CreditApplications.Shared;

internal static class CreditApplicationConductorNames
{
    public const string WorkflowName = "credit-application";
    public const int WorkflowVersion = 1;

    public const string SimulationTask = "credit_simulation";
    public const string CustomerVerificationTask = "credit_customer_verification";
    public const string DecisionTask = "credit_decision";
    public const string CloseApplicationTask = "credit_close_application";

    public const string WaitSimulationTask = "wait_simulation_finished";
    public const string WaitCustomerVerificationTask = "wait_customer_verified";
    public const string WaitDecisionTask = "wait_decision_generated";
    public const string WaitContractSignedTask = "wait_contract_signed";
    public const string WaitApplicationClosedTask = "wait_application_closed";
}
