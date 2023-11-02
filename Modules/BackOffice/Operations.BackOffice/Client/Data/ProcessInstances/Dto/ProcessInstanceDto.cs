using Operations.BackOffice.Client.Data.ProcessInstances.Dto;

public record ProcessInstanceDto
{
    public long? Key { get; init; }

    public int? ProcessVersion { get; init; }

    public string? BpmnProcessId { get; init; }

    public long? ProcessDefinitionKey { get; init; }


    public DateTimeOffset? StartDate { get; init; }

    public DateTimeOffset? EndDate { get; init; }

    public ProcessInstanceState? State { get; init; }
}
