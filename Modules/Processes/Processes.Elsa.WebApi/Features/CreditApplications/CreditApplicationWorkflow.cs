using Applications.Contracts.Events;
using Calculations.Contracts;
using Elsa.Extensions;
using Elsa.Scheduling.Activities;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Activities.Flowchart.Activities;
using Elsa.Workflows.Activities.Flowchart.Models;
using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime.ActivationValidators;
using Processes.Elsa.WebApi.Domain.CreditApplications;
using Processes.Elsa.WebApi.Features.CreditApplications.Close;
using Processes.Elsa.WebApi.Features.CreditApplications.CustomerVerification;
using Processes.Elsa.WebApi.Features.CreditApplications.Decision;
using Processes.Elsa.WebApi.Features.CreditApplications.Simulation;
using FlowEndpoint = Elsa.Workflows.Activities.Flowchart.Models.Endpoint;
using Event = Elsa.Workflows.Runtime.Activities.Event;
using DecisionStatus = Common.Application.Dictionary.Decision;
using ExpressionExecutionContext =
    global::Elsa.Expressions.Models.ExpressionExecutionContext;

namespace Processes.Elsa.WebApi.Features.CreditApplications;

public sealed class CreditApplicationWorkflow : WorkflowBase
{
    public const string DefinitionId = "credit-application";
    public const string DefinitionVersionId = $"{nameof(CreditApplicationWorkflow)}:v1";
    public const string ProcessInputName = "ProcessInstance";

    private const DecisionStatus PositiveDecision = DecisionStatus.Positive;

    protected override void Build(IWorkflowBuilder builder)
    {
        builder.WithDefinitionId(DefinitionId);
        builder.Name = "Credit Application";
        builder.Description = "Credit application orchestration implemented with Elsa Workflows.";
        builder.Version = 1;

        builder.WithActivationStrategyType<CorrelationStrategy>();

        // Variables
        var simulationResult = builder
            .WithVariable<SimulationFinished?>("SimulationResult", null)
            .WithWorkflowStorage();

        var customerVerificationResult = builder
            .WithVariable<CustomerVerified?>("CustomerVerificationResult", null)
            .WithWorkflowStorage();

        var decisionResult = builder
            .WithVariable<DecisionGenerated?>("DecisionResult", null)
            .WithWorkflowStorage();

        var processInstance = builder
            .WithVariable<CreditProcessInstance?>(ProcessInputName, null)
            .WithWorkflowStorage();

        // Helpers
        CreditProcessInstance GetWorkflowProcessInput(ExpressionExecutionContext context) => context
            .GetActivityExecutionContext()
            .GetWorkflowInput<CreditProcessInstance>(ProcessInputName);

        Input<CreditProcessInstance> ProcessInput()
        {
            return new Input<CreditProcessInstance>(
                context => processInstance.Get(context)!);
        }

        Input<string> ApplicationId()
        {
            return new Input<string>(
                context => processInstance.Get(context)!.ApplicationId);
        }

        Event WaitForApplicationClosed(string name) => new(CreditApplicationEventNames.ApplicationClosed)
        {
            Name = name
        };

        Connection Connect(IActivity source, IActivity target) => new(source, target);

        Connection ConnectBranch(IActivity source, string sourcePort, IActivity target) => new(
            new FlowEndpoint(source, sourcePort),
            new FlowEndpoint(target));

        var initializeProcessInstance = new SetVariable<CreditProcessInstance?>(
            processInstance,
            context => GetWorkflowProcessInput(context))
        {
            Name = "Initialize Process Instance"
        };

        // Process and cancellation watchers.
        // The process runs alongside two terminal watchers. The first branch to
        // reach Process Finished completes the workflow.
        var startProcessAndCancellationWatchers = new FlowFork
        {
            Name = "Process",
            Branches = new(new[]
            {
                "Main",
                "Cancellation",
                "Timeout"
            })
        };

        // Main process
        var startVerifications = new FlowFork
        {
            Name = "Start Verifications",
            Branches = new Input<ICollection<string>>([
                "Simulation",
                "Verification"
            ])
        };

        // Simulation
        var publishSimulation = new PublishSimulationActivity
        {
            Name = "Publish Simulation",
            ProcessInstance = ProcessInput()
        };

        var waitForSimulationResult = new Event(CreditApplicationEventNames.SimulationFinished)
        {
            Name = "Simulation Finished",
            Result = new Output<object?>(simulationResult)
        };

        // Customer Verification
        var publishCustomerVerification = new PublishCustomerVerificationActivity
        {
            Name = "Publish Customer Verification",
            ProcessInstance = ProcessInput()
        };

        var waitForCustomerVerificationResult = new Event(CreditApplicationEventNames.CustomerVerified)
        {
            Name = "Customer Verified",
            Result = new Output<object?>(customerVerificationResult)
        };

        // Wait for both checks.
        var waitForVerifications = new FlowJoin
        {
            Name = "Verifications Completed",
            Mode = new(FlowJoinMode.WaitAll)
        };

        // Decision
        var publishDecision = new PublishDecisionActivity
        {
            Name = "Publish Decision",
            ApplicationId = ApplicationId(),
            SimulationStatus = new Input<DecisionStatus>(context => simulationResult.Get(context)!.SimulationStatus),
            CustomerVerificationStatus = new Input<DecisionStatus>(context =>
                customerVerificationResult.Get(context)!.CustomerVerificationStatus)
        };

        var waitForDecisionResult = new Event(CreditApplicationEventNames.DecisionGenerated)
        {
            Name = "Decision Generated",
            Result = new Output<object?>(decisionResult)
        };

        // Decision result

        var decision = new FlowDecision(
            context => decisionResult.Get(context)!.Decision == PositiveDecision)
        {
            Name = "Positive Decision?"
        };

        // Positive decision

        var waitForContractSignature = new Event(CreditApplicationEventNames.ContractSigned)
        {
            Name = "Contract Signed"
        };

        // Negative decision

        var closeAfterNegativeDecision = new PublishCloseApplicationActivity
        {
            Name = "Close Application",
            ApplicationId = ApplicationId()
        };

        var waitForCloseAfterNegativeDecision = WaitForApplicationClosed("Application Closed");

        // Cancellation

        var waitForCancellation = new Event(CreditApplicationEventNames.ApplicationCancelled)
        {
            Name = "Application Cancelled"
        };

        // Timeout

        var waitForTimeout = new Delay(context => processInstance.Get(context)!.Timeout)
        {
            Name = "Application Timeout"
        };

        var closeAfterTimeout = new PublishCloseApplicationActivity
        {
            Name = "Close After Timeout",
            ApplicationId = ApplicationId()
        };

        var waitForCloseAfterTimeout = WaitForApplicationClosed("Closed After Timeout");

        // Process completion

        var completeProcess = new FlowJoin
        {
            Name = "Process Finished",
            Mode = new(FlowJoinMode.WaitAny)
        };

        // Finish

        var finish = new Finish
        {
            Name = "Finish Credit Application"
        };

        // Flowchart

        builder.Root = new Flowchart
        {
            Activities =
            {
                // Process and cancellation watchers
                initializeProcessInstance,
                startProcessAndCancellationWatchers,

                // Main process
                startVerifications,

                publishSimulation,
                waitForSimulationResult,

                publishCustomerVerification,
                waitForCustomerVerificationResult,

                waitForVerifications,

                publishDecision,
                waitForDecisionResult,
                decision,

                waitForContractSignature,

                closeAfterNegativeDecision,
                waitForCloseAfterNegativeDecision,

                // Cancellation
                waitForCancellation,

                // Timeout
                waitForTimeout,
                closeAfterTimeout,
                waitForCloseAfterTimeout,

                // Process completion
                completeProcess,

                finish
            },

            Connections =
            {
                // Process and cancellation watchers

                Connect(initializeProcessInstance, startProcessAndCancellationWatchers),

                ConnectBranch(startProcessAndCancellationWatchers, "Main", startVerifications),

                ConnectBranch(startProcessAndCancellationWatchers, "Cancellation", waitForCancellation),

                ConnectBranch(startProcessAndCancellationWatchers, "Timeout", waitForTimeout),

                // Credit checks

                ConnectBranch(startVerifications, "Simulation", publishSimulation),

                ConnectBranch(startVerifications, "Verification", publishCustomerVerification),

                // Simulation

                Connect(publishSimulation, waitForSimulationResult),

                Connect(waitForSimulationResult, waitForVerifications),

                // Customer verification

                Connect(publishCustomerVerification, waitForCustomerVerificationResult),

                Connect(waitForCustomerVerificationResult, waitForVerifications),

                // Decision

                Connect(waitForVerifications, publishDecision),

                Connect(publishDecision, waitForDecisionResult),

                Connect(waitForDecisionResult, decision),

                // Positive decision

                ConnectBranch(decision, "True", waitForContractSignature),

                Connect(waitForContractSignature, completeProcess),

                // Negative decision

                ConnectBranch(decision, "False", closeAfterNegativeDecision),

                Connect(closeAfterNegativeDecision, waitForCloseAfterNegativeDecision),

                Connect(waitForCloseAfterNegativeDecision, completeProcess),

                // Cancellation

                Connect(waitForCancellation, completeProcess),

                // Timeout

                Connect(waitForTimeout, closeAfterTimeout),

                Connect(closeAfterTimeout, waitForCloseAfterTimeout),

                Connect(waitForCloseAfterTimeout, completeProcess),

                // Finish

                Connect(completeProcess, finish)
            }
        };
    }
}
