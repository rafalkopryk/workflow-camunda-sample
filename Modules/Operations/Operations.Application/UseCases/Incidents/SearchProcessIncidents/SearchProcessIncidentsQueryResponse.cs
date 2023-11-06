using Operations.Application.UseCases.ProcessDefinitions.GetProcessDefinitionXml;

namespace Operations.Application.UseCases.Incidents.SearchProcessIncidents;

public record SearchProcessIncidentsQueryResponse(ProcessIncidentDto[] Items, object[] SortValues);
