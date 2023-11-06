namespace Operations.BackOffice.Client.Data.Incidents.Dto;

public record ProcessIncidentDto
{
    public long? Key { get; init; }
    public long? ProcessDefinitionKey { get; init; }
    public long? ProcessInstanceKey { get; init; }
    public string? Message { get; init; }
    public DateTimeOffset? CreationTime { get; init; }
    public ProcessIncidenState? State { get; init; }
    public string? ProcessElementId { get; init; }
}

public enum ProcessIncidenState
{
    ACTIVE,
    RESOLVED,
    PENDING,
}
