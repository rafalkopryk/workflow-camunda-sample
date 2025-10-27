using Processes.Temporal.WebApi.Domain.CreditApplications;
using Processes.Temporal.WebApi.UseCases.CreditApplications.Close;
using Processes.Temporal.WebApi.UseCases.CreditApplications.CustomerVerification;
using Processes.Temporal.WebApi.UseCases.CreditApplications.Decision;
using Processes.Temporal.WebApi.UseCases.CreditApplications.Simulation;
using Temporalio.Workflows;

namespace Processes.Temporal.WebApi.UseCases.CreditApplications;

[Workflow]
public class CreditApplicationWorkflow()
{
    private CreditProcessInstance _processInstance;
    private bool _simulationCompleted;
    private bool _customerVerificationCompleted;
    private bool _decisionCompleted;
    private bool _contractSigned;
    private bool _cancelled;

    [WorkflowRun]
    public async Task<CreditProcessInstance> RunAsync(CreditProcessInstance processInstance)
    {
        _processInstance = processInstance;

        var startSimulationTask = Workflow.ExecuteActivityAsync(
            (SimulationService service) => service.StartSimulation(_processInstance),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        var startCustomerVerificationTask =  Workflow.ExecuteActivityAsync(
            (CustomerVerificationService service) => service.StartCustomerVerification(_processInstance),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );
        
        await Workflow.WhenAllAsync(startSimulationTask, startCustomerVerificationTask);
        
        await Workflow.WaitConditionAsync(() => (_simulationCompleted && _customerVerificationCompleted) || _cancelled);
        if (_cancelled)
        {
            return _processInstance;
        }

        await Workflow.ExecuteActivityAsync(
            (DecisionService service) => service.Handle(_processInstance),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        await Workflow.WaitConditionAsync(() => _decisionCompleted);
        if (_cancelled)
        {
            return _processInstance;
        }

        if (_processInstance.Decision != "Positive")
        {
            await Workflow.ExecuteActivityAsync(
                (CloseApplicationService service) => service.Handle(_processInstance.ApplicationId),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );

            await Workflow.WaitConditionAsync(() => _cancelled);
            
            return _processInstance;
        }
        
        await Workflow.WaitConditionAsync(() => _contractSigned || _cancelled);
        
        return _processInstance;
    }

    [WorkflowSignal]
    public Task OnSimulationCompletedAsync(string simulationStatus)
    {
        _processInstance = _processInstance with { SimulationStatus = simulationStatus };
        _simulationCompleted = true;
        return Task.CompletedTask;
    }

    [WorkflowSignal]
    public Task OnCustomerVerificationCompletedAsync(string customerVerificationStatus)
    {
        _processInstance = _processInstance with { CustomerVerificationStatus = customerVerificationStatus };
        _customerVerificationCompleted = true;
        return Task.CompletedTask;
    }

    [WorkflowSignal]
    public Task OnDecisionCompletedAsync(string decision)
    {
        _processInstance = _processInstance with { Decision = decision };
        _decisionCompleted = true;
        return Task.CompletedTask;
    }
    
    [WorkflowSignal]
    public Task OnContractSignedAsync()
    {
        _contractSigned = true;
        return Task.CompletedTask;
    }

    [WorkflowSignal]
    public Task OnCancelledAsync()
    {
        _cancelled = true;
        return Task.CompletedTask;
    }

    [WorkflowQuery]
    public CreditProcessInstance GetProcessInstance() => _processInstance;
}