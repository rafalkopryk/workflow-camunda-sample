namespace Operations.Application.UseCases.Incidents.Shared;

internal static class ProcessIncidentKeyword
{
    public const string INDEX = "zeebe-record-incident";
    public const string INTENT_CREATED = "CREATED";
    public const string INTENT_RESOLVED = "RESOLVED";

    public const string VALUETYPE = "INCIDENT";
}