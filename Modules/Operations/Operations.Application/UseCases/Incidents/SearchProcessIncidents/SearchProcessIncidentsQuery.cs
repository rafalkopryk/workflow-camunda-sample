using CSharpFunctionalExtensions;
using MediatR;
using Operations.Application.UseCases.ProcessDefinitions.GetProcessDefinitionXml;

namespace Operations.Application.UseCases.Incidents.SearchProcessIncidents;

public record SearchProcessIncidentsQuery(ProcessIncidentDto? Filter, object[]? SearchAfter, int? Size = 50) : IRequest<Result<SearchProcessIncidentsQueryResponse>>;
