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
using Endpoint = Elsa.Workflows.Activities.Flowchart.Models.Endpoint;
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
        builder.Description =
            "Credit application orchestration implemented with Elsa Workflows.";
        builder.Version = 1;

        builder.WithActivationStrategyType<CorrelationStrategy>();

        //
        // Variables
        //
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

        //
        // Helpers
        //
        CreditProcessInstance GetWorkflowProcessInput(ExpressionExecutionContext context)
        {
            return context
                .GetActivityExecutionContext()
                .GetWorkflowInput<CreditProcessInstance>(
                    ProcessInputName);
        }

        Input<CreditProcessInstance> ProcessInput()
        {
            return new Input<CreditProcessInstance>(
                context => processInstance.Get(context)!);
        }

        var initializeProcessInstance = new SetVariable<CreditProcessInstance?>(
            processInstance,
            context => GetWorkflowProcessInput(context))
        {
            Name = "Initialize Process Instance"
        };

        //
        // =========================================================
        // OUTER PROCESS RACE
        //
        // Main process
        // External close
        // Timeout
        // =========================================================
        //
        var processFork = new FlowFork
        {
            Name = "Process",

            Branches = new(new[]
            {
                "Main",
                "ExternalClose",
                "Timeout"
            })
        };

        //
        // =========================================================
        // MAIN PROCESS
        // =========================================================
        //
        var checksFork = new FlowFork
        {
            Name = "Start Checks",
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

        var simulationFinished = new Event(CreditApplicationEventNames.SimulationFinished)
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

        var customerVerified = new Event(CreditApplicationEventNames.CustomerVerified)
        {
            Name = "Customer Verified",
            Result = new Output<object?>(customerVerificationResult)
        };

        //
        // Wait for BOTH checks.
        //
        var checksJoin = new FlowJoin
        {
            Name = "Checks Completed",
            Mode = new(FlowJoinMode.WaitAll)
        };

        //
        // Decision
        //
        var publishDecision = new PublishDecisionActivity
        {
            Name = "Publish Decision",
            ApplicationId = new Input<string>(context => processInstance.Get(context)!.ApplicationId),
            SimulationStatus = new Input<DecisionStatus>(context =>
                Enum.Parse<DecisionStatus>(
                    simulationResult.Get(context)!.SimulationStatus,
                    ignoreCase: true)),

            CustomerVerificationStatus =
                new Input<DecisionStatus>(context =>
                    customerVerificationResult.Get(context)!
                        .CustomerVerificationStatus)
        };

        var decisionGenerated =
            new Event(
                CreditApplicationEventNames.DecisionGenerated)
            {
                Name = "Decision Generated",

                Result =
                    new Output<object?>(
                        decisionResult)
            };

        //
        // Positive / Negative
        //

        var decision =
            new FlowDecision(
                context =>
                    decisionResult.Get(context)!.Decision == PositiveDecision)
            {
                Name = "Positive Decision?"
            };

        //
        // Positive
        //

        var contractSigned =
            new Event(
                CreditApplicationEventNames.ContractSigned)
            {
                Name = "Contract Signed"
            };

        //
        // Negative
        //

        var closeAfterNegativeDecision =
            new PublishCloseApplicationActivity
            {
                Name = "Close Application",

                ApplicationId =
                    new Input<string>(
                        context =>
                            processInstance.Get(context)!
                                .ApplicationId)
            };

        var applicationClosedAfterNegativeDecision =
            new Event(
                CreditApplicationEventNames.ApplicationClosed)
            {
                Name = "Application Closed"
            };

        //
        // End of main process.
        //

        var mainFinished = new FlowJoin
        {
            Name = "Main Process Finished",

            //
            // Positive OR negative path.
            //
            Mode = new(FlowJoinMode.WaitAny)
        };

        //
        // =========================================================
        // EXTERNAL CLOSE
        // =========================================================
        //

        var externallyClosed =
            new Event(
                CreditApplicationEventNames.ApplicationClosed)
            {
                Name = "External Application Closed"
            };

        //
        // =========================================================
        // TIMEOUT
        // =========================================================
        //

        var timeoutDelay =
            new Delay(
                context =>
                    processInstance.Get(context)!
                        .Timeout)
            {
                Name = "Application Timeout"
            };

        var closeAfterTimeout =
            new PublishCloseApplicationActivity
            {
                Name = "Close After Timeout",

                ApplicationId =
                    new Input<string>(
                        context =>
                            processInstance.Get(context)!
                                .ApplicationId)
            };

        var applicationClosedAfterTimeout =
            new Event(
                CreditApplicationEventNames.ApplicationClosed)
            {
                Name = "Closed After Timeout"
            };

        //
        // =========================================================
        // OUTER WAIT ANY
        // =========================================================
        //

        var processFinished =
            new FlowJoin
            {
                Name = "Process Finished",

                Mode = new(FlowJoinMode.WaitAny)
            };

        //
        // Finish node.
        //

        var finish =
            new Finish
            {
                Name = "Finish Credit Application"
            };

        //
        // =========================================================
        // FLOWCHART
        // =========================================================
        //

        builder.Root = new Flowchart
        {
            Activities =
            {
                //
                // Outer race
                //
                initializeProcessInstance,
                processFork,

                //
                // Main
                //
                checksFork,

                publishSimulation,
                simulationFinished,

                publishCustomerVerification,
                customerVerified,

                checksJoin,

                publishDecision,
                decisionGenerated,
                decision,

                contractSigned,

                closeAfterNegativeDecision,
                applicationClosedAfterNegativeDecision,

                mainFinished,

                //
                // External close
                //
                externallyClosed,

                //
                // Timeout
                //
                timeoutDelay,
                closeAfterTimeout,
                applicationClosedAfterTimeout,

                //
                // Outer join
                //
                processFinished,

                finish
            },

            Connections =
            {
                //
                // =================================================
                // OUTER FORK
                // =================================================
                //

                new(
                    initializeProcessInstance,
                    processFork),

                new(
                    new Endpoint(processFork, "Main"),
                    new Endpoint(checksFork)),

                new(
                    new Endpoint(processFork, "ExternalClose"),
                    new Endpoint(externallyClosed)),

                new(
                    new Endpoint(processFork, "Timeout"),
                    new Endpoint(timeoutDelay)),

                //
                // =================================================
                // CHECKS FORK
                // =================================================
                //

                new(
                    new Endpoint(checksFork, "Simulation"),
                    new Endpoint(publishSimulation)),

                new(
                    new Endpoint(checksFork, "Verification"),
                    new Endpoint(publishCustomerVerification)),

                //
                // Simulation path
                //

                new(
                    publishSimulation,
                    simulationFinished),

                new(
                    simulationFinished,
                    checksJoin),

                //
                // Verification path
                //

                new(
                    publishCustomerVerification,
                    customerVerified),

                new(
                    customerVerified,
                    checksJoin),

                //
                // =================================================
                // DECISION
                // =================================================
                //

                new(
                    checksJoin,
                    publishDecision),

                new(
                    publishDecision,
                    decisionGenerated),

                new(
                    decisionGenerated,
                    decision),

                //
                // Positive
                //

                new(
                    new Endpoint(decision, "True"),
                    new Endpoint(contractSigned)),

                new(
                    contractSigned,
                    mainFinished),

                //
                // Negative
                //

                new(
                    new Endpoint(decision, "False"),
                    new Endpoint(closeAfterNegativeDecision)),

                new(
                    closeAfterNegativeDecision,
                    applicationClosedAfterNegativeDecision),

                new(
                    applicationClosedAfterNegativeDecision,
                    mainFinished),

                //
                // =================================================
                // MAIN -> OUTER JOIN
                // =================================================
                //

                new(
                    mainFinished,
                    processFinished),

                //
                // =================================================
                // EXTERNAL CLOSE -> OUTER JOIN
                // =================================================
                //

                new(
                    externallyClosed,
                    processFinished),

                //
                // =================================================
                // TIMEOUT
                // =================================================
                //

                new(
                    timeoutDelay,
                    closeAfterTimeout),

                new(
                    closeAfterTimeout,
                    applicationClosedAfterTimeout),

                new(
                    applicationClosedAfterTimeout,
                    processFinished),

                //
                // =================================================
                // FINISH
                // =================================================
                //

                new(
                    processFinished,
                    finish)
            }
        };
    }
}
