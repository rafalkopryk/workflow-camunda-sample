using Operations.Application.UseCases.ProcessInstances.Shared.Dto;

public class ProcessInstanceDto
{
    public long? Key { get; set; }

    public int? ProcessVersion { get; set; }

    public string? BpmnProcessId { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public ProcessInstanceState? State { get; set; }
}
