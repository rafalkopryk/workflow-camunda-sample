using Conductor.Client.Models;
using Conductor.Definition;
using Conductor.Definition.TaskType;
using Processes.Conductor.WebApi.Features.CreditApplications.Shared;

namespace Processes.Conductor.WebApi.ConductorDefinitions;

internal static class CreditApplicationWorkflowDefinition
{
    private const string OwnerEmail = "workflow-sample@local";

    public static ConductorWorkflow Create()
    {
        var workflow = new ConductorWorkflow
        {
            // conductor-csharp 3.0.0 does not initialize this collection.
            OutputParameters = [],
        }
            .WithName(CreditApplicationConductorNames.WorkflowName)
            .WithDescription("Credit application process translated from the Camunda BPMN and Temporal workflow.")
            .WithVersion(CreditApplicationConductorNames.WorkflowVersion)
            .WithOwner(OwnerEmail)
            .WithInputParameter("applicationId")
            .WithInputParameter("documentId")
            .WithInputParameter("amount")
            .WithInputParameter("creditPeriodInMonths")
            .WithInputParameter("averageNetMonthlyIncome")
            .WithRestartable(true)
            .WithTimeoutPolicy(WorkflowDef.TimeoutPolicyEnum.ALERTONLY, 0);

        var simulationRequest = new SimpleTask(
                CreditApplicationConductorNames.SimulationTask,
                "request_simulation")
            .WithInput("applicationId", workflow.Input("applicationId"))
            .WithInput("amount", workflow.Input("amount"))
            .WithInput("creditPeriodInMonths", workflow.Input("creditPeriodInMonths"))
            .WithInput("averageNetMonthlyIncome", workflow.Input("averageNetMonthlyIncome"));
        var simulationFinished = CreateWaitTask(
            "simulation_finished",
            CreditApplicationConductorNames.WaitSimulationTask,
            workflow.Input("applicationId"));

        var customerVerificationRequest = new SimpleTask(
                CreditApplicationConductorNames.CustomerVerificationTask,
                "request_customer_verification")
            .WithInput("applicationId", workflow.Input("applicationId"))
            .WithInput("documentId", workflow.Input("documentId"));
        var customerVerified = CreateWaitTask(
            "customer_verified",
            CreditApplicationConductorNames.WaitCustomerVerificationTask,
            workflow.Input("applicationId"));

        var verifications = new ForkJoinTask(
            "start_verifications",
            [simulationRequest],
            [simulationFinished],
            [customerVerificationRequest],
            [customerVerified]);
        var joinVerifications = new JoinTask(
            "join_verifications",
            simulationRequest,
            simulationFinished,
            customerVerificationRequest,
            customerVerified);

        var decisionRequest = new SimpleTask(
                CreditApplicationConductorNames.DecisionTask,
                "request_decision")
            .WithInput("applicationId", workflow.Input("applicationId"))
            .WithInput("simulationStatus", simulationFinished.Output("simulationStatus"))
            .WithInput("customerVerificationStatus", customerVerified.Output("customerVerificationStatus"));
        var decisionGenerated = CreateWaitTask(
            "decision_generated",
            CreditApplicationConductorNames.WaitDecisionTask,
            workflow.Input("applicationId"));
        var generateDecision = new ForkJoinTask(
            "generate_decision",
            [decisionRequest],
            [decisionGenerated]);
        var joinDecision = new JoinTask(
            "join_decision",
            decisionRequest,
            decisionGenerated);

        var contractSigned = CreateHumanTask(
            "contract_signed",
            CreditApplicationConductorNames.WaitContractSignedTask,
            workflow.Input("applicationId"));
        var closeApplication = new SimpleTask(
                CreditApplicationConductorNames.CloseApplicationTask,
                "request_close_application")
            .WithInput("applicationId", workflow.Input("applicationId"))
            .WithInput("reason", "NegativeDecision");
        var waitApplicationClosed = CreateWaitTask(
            "wait_application_closed",
            CreditApplicationConductorNames.WaitApplicationClosedTask,
            workflow.Input("applicationId"));
        var applicationClosed = new TerminateTask(
            "application_closed",
            WorkflowStatus.StatusEnum.COMPLETED,
            terminationReason: "Application closed");
        var closeApplicationAndWait = new ForkJoinTask(
            "close_application_and_wait",
            [closeApplication],
            [waitApplicationClosed]);
        var joinApplicationClosed = new JoinTask(
            "join_application_closed",
            closeApplication,
            waitApplicationClosed);

        var routeDecision = new SwitchTask(
                "route_decision",
                decisionGenerated.Output("decision"))
            .WithDecisionCase("Positive", contractSigned)
            .WithDefaultCase(closeApplicationAndWait, joinApplicationClosed, applicationClosed);

        workflow
            .WithTask(verifications)
            .WithTask(joinVerifications)
            .WithTask(generateDecision)
            .WithTask(joinDecision)
            .WithTask(routeDecision)
            .WithOutputParameter("applicationId", workflow.Input("applicationId"))
            .WithOutputParameter("simulationStatus", simulationFinished.Output("simulationStatus"))
            .WithOutputParameter("customerVerificationStatus", customerVerified.Output("customerVerificationStatus"))
            .WithOutputParameter("decision", decisionGenerated.Output("decision"));

        workflow.SchemaVersion = 2;
        workflow.WorkflowStatusListenerEnabled = false;

        return workflow;
    }

    private static WaitTask CreateWaitTask(string name, string referenceName, string applicationId)
    {
        // The SDK requires a timer when constructing WaitTask. These waits are instead
        // completed by domain-event handlers, so remove the generated timer input.
        var task = new WaitTask(referenceName, TimeSpan.Zero)
        {
            Name = name,
        };
        task.InputParameters.Remove("duration");
        task.WithInput("applicationId", applicationId);

        return task;
    }

    private static HumanTask CreateHumanTask(string name, string referenceName, string applicationId)
    {
        var task = new HumanTask(referenceName)
        {
            Name = name,
        };
        task.WithInput("applicationId", applicationId);

        return task;
    }
}
