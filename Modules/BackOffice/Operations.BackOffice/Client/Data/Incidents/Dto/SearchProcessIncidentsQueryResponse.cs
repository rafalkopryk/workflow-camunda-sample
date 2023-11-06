namespace Operations.BackOffice.Client.Data.Incidents.Dto;

public record SearchProcessIncidentsQueryResponse(ProcessIncidentDto[] Items, object[] SortValues);
