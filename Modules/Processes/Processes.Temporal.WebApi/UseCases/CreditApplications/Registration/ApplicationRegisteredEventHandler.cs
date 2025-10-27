using Processes.Temporal.WebApi.Domain.CreditApplications;
using Temporalio.Client;
using Wolverine.Attributes;
using Task = System.Threading.Tasks.Task;

namespace Processes.Temporal.WebApi.UseCases.CreditApplications.Registration;

[MessageIdentity("applicationRegistered", Version = 1)]
public record ApplicationRegistered(string ApplicationId, decimal Amount, int CreditPeriodInMonths, decimal AverageNetMonthlyIncome, string DocumentId);

[WolverineHandler]
public class ApplicationRegisteredEventHandler(ITemporalClient client)
{
    public async Task Handle(ApplicationRegistered message)
    {
        var processInstance = new CreditProcessInstance
        {
            ApplicationId = message.ApplicationId,
            DocumentId = message.DocumentId,
            Amount = message.Amount,
            CreditPeriodInMonths = message.CreditPeriodInMonths,
            AverageNetMonthlyIncome = message.AverageNetMonthlyIncome,
        };
        
        await client.StartWorkflowAsync<CreditApplicationWorkflow>(
            x => x.RunAsync(processInstance),
            new WorkflowOptions(processInstance.ApplicationId, CreditApplicationKeyword.CREDIT_APPLICATION_TASK_QUEUE));
    }
}
