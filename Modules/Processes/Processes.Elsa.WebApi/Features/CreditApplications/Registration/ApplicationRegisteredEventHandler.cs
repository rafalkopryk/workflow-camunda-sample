using Applications.Contracts.Events;
using Elsa.Workflows.Runtime;
using Microsoft.Extensions.Options;
using Processes.Elsa.WebApi.Domain.CreditApplications;
using Wolverine.Attributes;
using DispatchWorkflowDefinitionRequest = Elsa.Workflows.Runtime.Requests.DispatchWorkflowDefinitionRequest;
using Task = System.Threading.Tasks.Task;

namespace Processes.Elsa.WebApi.Features.CreditApplications.Registration;

[WolverineHandler]
public sealed class ApplicationRegisteredEventHandler(
    IWorkflowDispatcher workflowDispatcher,
    IOptions<CreditApplicationWorkflowOptions> creditApplicationWorkflowOptions)
{
    public async Task Handle(ApplicationRegistered message, CancellationToken cancellationToken)
    {
        var process = new CreditProcessInstance
        {
            ApplicationId = message.ApplicationId,
            DocumentId = message.DocumentId,
            Amount = message.Amount,
            CreditPeriodInMonths = message.CreditPeriodInMonths,
            AverageNetMonthlyIncome = message.AverageNetMonthlyIncome,
            Timeout = creditApplicationWorkflowOptions.Value.Timeout,
        };

        var request = new DispatchWorkflowDefinitionRequest()
        {
            DefinitionVersionId = CreditApplicationWorkflow.DefinitionVersionId,
            CorrelationId =message.ApplicationId,
            Input = new Dictionary<string, object>
            {
                [CreditApplicationWorkflow.ProcessInputName] = process,
            }
        };

        var options = new DispatchWorkflowOptions
        {
            Channel = null
        };

        await workflowDispatcher.DispatchAsync(request, options, cancellationToken);
    }
}
