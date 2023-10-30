namespace Operations.BackOffice.Client.Dto.ProcessDefinitions;

public record ProcessDefinitionDto
{
    public long? Key { get; init; }

    public string? Name { get; init; }

    public int? Version { get; init; }

    public string? BpmnProcessId { get; init; }
}
