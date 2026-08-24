using Conductor.Client.Models;
using Processes.Conductor.WebApi.Features.CreditApplications.Shared;

namespace Processes.Conductor.WebApi.ConductorDefinitions;

internal static class CreditApplicationTaskDefinitions
{
    private const string OwnerEmail = "workflow-sample@local";

    public static List<TaskDef> Create() =>
    [
        CreateTask(
            CreditApplicationConductorNames.SimulationTask,
            "Publish a request to calculate the credit simulation."),
        CreateTask(
            CreditApplicationConductorNames.CustomerVerificationTask,
            "Publish a request to verify the credit customer."),
        CreateTask(
            CreditApplicationConductorNames.DecisionTask,
            "Publish a request to generate the credit decision."),
        CreateTask(
            CreditApplicationConductorNames.CloseApplicationTask,
            "Publish a request to close the credit application."),
    ];

    private static TaskDef CreateTask(string name, string description) => new()
    {
        Name = name,
        Description = description,
        RetryCount = 3,
        RetryLogic = TaskDef.RetryLogicEnum.FIXED,
        RetryDelaySeconds = 5,
        TimeoutSeconds = 300,
        ResponseTimeoutSeconds = 300,
        TimeoutPolicy = TaskDef.TimeoutPolicyEnum.TIMEOUTWF,
        OwnerEmail = OwnerEmail,
    };
}
