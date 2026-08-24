using Conductor.Api;
using Conductor.Client.Models;
using Processes.Conductor.WebApi.Features.CreditApplications.Shared;
using Wolverine.Attributes;
using Task = System.Threading.Tasks.Task;

namespace Processes.Conductor.WebApi.Features.CreditApplications.Registration;

[MessageIdentity("applicationRegistered", Version = 1)]
public record ApplicationRegistered(
    string ApplicationId,
    decimal Amount,
    int CreditPeriodInMonths,
    decimal AverageNetMonthlyIncome,
    string DocumentId);

[WolverineHandler]
public class ApplicationRegisteredEventHandler(WorkflowResourceApi workflowApi)
{
    public async Task Handle(ApplicationRegistered message, CancellationToken cancellationToken)
    {
        var input = new Dictionary<string, object>
        {
            ["applicationId"] = message.ApplicationId,
            ["documentId"] = message.DocumentId,
            ["amount"] = message.Amount,
            ["creditPeriodInMonths"] = message.CreditPeriodInMonths,
            ["averageNetMonthlyIncome"] = message.AverageNetMonthlyIncome,
        };
        
        await workflowApi.StartWorkflowAsync(new StartWorkflowRequest(
            correlationId: message.ApplicationId,
            input: input,
            name: CreditApplicationConductorNames.WorkflowName,
            version: CreditApplicationConductorNames.WorkflowVersion,
            idempotencyKey: message.ApplicationId,
            idempotencyStrategy: IdempotencyStrategy.RETURN_EXISTING));
    }
}
