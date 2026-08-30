namespace Processes.Elsa.WebApi.Features.CreditApplications;

internal static class CreditApplicationEventNames
{
    public const string SimulationFinished = "credit-application.simulation-finished";
    public const string CustomerVerified = "credit-application.customer-verified";
    public const string DecisionGenerated = "credit-application.decision-generated";
    public const string ContractSigned = "credit-application.contract-signed";
    public const string ApplicationCancelled = "credit-application.application-cancelled";
    public const string ApplicationClosed = "credit-application.application-closed";
}
