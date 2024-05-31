namespace CreditProcessFunctions.UseCases;

public static class TopicAttributes
{
    public const string SIMULATION = "command.credit.calculations.simulation.v1";
    public const string DECISION = "command.credit.applications.decision.v1";
    public const string CLOSE = "command.credit.applications.close.v1";

    public static readonly string[] Produces = [SIMULATION, DECISION, CLOSE];

    public const string APPLICATIONREGISTERED = "event.credit.applications.applicationregistered.v1";
    public const string APPLICATIONREGISTERED_SUBSCRIPTION = "applicationregistered-registration-function";

    public const string SIMULATIONFINISHED = "event.credit.calculations.simulationFinished.v1";
    public const string SIMULATIONFINISHED_SUBSCRIPTION = "simulationFinished-simulation-function";

    public const string DECISIONGENERATED = "event.credit.applications.decisionGenerated.v1";
    public const string DECISIONGENERATED_SUBSCRIPTION = "decisionGenerated-decision-function";

    public const string CONTRACTSIGNED = "event.credit.applications.contractSigned";
    public const string CONTRACTSIGNED_SUBSCRIPTION = "contractSigned-contract-function";

    public static readonly (string Topic, string Subcription)[] Receives = 
    [
        (APPLICATIONREGISTERED, APPLICATIONREGISTERED_SUBSCRIPTION),
        (SIMULATIONFINISHED, SIMULATIONFINISHED_SUBSCRIPTION),
        (DECISIONGENERATED, DECISIONGENERATED_SUBSCRIPTION),
        (CONTRACTSIGNED, CONTRACTSIGNED_SUBSCRIPTION)
    ];
}
