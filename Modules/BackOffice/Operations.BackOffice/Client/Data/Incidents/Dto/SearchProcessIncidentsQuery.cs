namespace Operations.BackOffice.Client.Data.Incidents.Dto;

public record SearchProcessIncidentsQuery(ProcessIncidentDto? Filter, object[]? SearchAfter = null, int? Size = 50);
