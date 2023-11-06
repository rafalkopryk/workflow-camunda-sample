using CSharpFunctionalExtensions;
using MediatR;
using Nest;
using Operations.Application.Incidents.SearchProcessIncidents.Shared.Documents;
using Operations.Application.UseCases.ProcessDefinitions.GetProcessDefinitionXml;

namespace Operations.Application.UseCases.Incidents.SearchProcessIncidents;

internal class SearchProcessIncidentsQueryHandler : IRequestHandler<SearchProcessIncidentsQuery, Result<SearchProcessIncidentsQueryResponse>>
{
    private readonly ElasticClient _elasticsearchClient;

    public SearchProcessIncidentsQueryHandler(ElasticClient elasticsearchClient)
    {
        _elasticsearchClient = elasticsearchClient;
    }

    public async Task<Result<SearchProcessIncidentsQueryResponse>> Handle(SearchProcessIncidentsQuery query, CancellationToken cancellationToken)
    {
        var incidents = new List<ProcessIncidentDto>();

        var serchAfter = query.SearchAfter;
        while (true)
        {
            var result = await _elasticsearchClient.SearchAsync<ProcessIncidentDocument>(s => s
                .Index("zeebe-record-incident")
                .Size(1000)
                .Sort(sort => sort
                    .Descending("timestamp"))
                .SearchAfter(serchAfter)
                .Query(q => +q
                     .Term(x => x.ValueType, "INCIDENT") && +q
                     .Terms(x => x.Field("intent").Terms("CREATED", "RESOLVED")) && +q
                     .Term(x => x.Value.ProcessDefinitionKey, query.Filter?.ProcessDefinitionKey) && +q
                     .Term(x => x.Value.ProcessInstanceKey, query.Filter?.ProcessInstanceKey) && +q
                     .Term(x => x.Key, query.Filter?.Key)));
            
            var incidentElementsGroupByKey = result.Documents.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToArray());
            var localIncidents = incidentElementsGroupByKey.Select(documents =>
            {
                var createdElement = documents.Value.FirstOrDefault(x => x.Intent == "CREATED");
                var resolvedElement = documents.Value.FirstOrDefault(x => x.Intent == "RESOLVED");

                var incident = new ProcessIncidentDto
                {
                    Key = documents.Key,
                    CreationTime = DateTimeOffset.FromUnixTimeMilliseconds(createdElement.Timestamp),
                    ProcessInstanceKey = createdElement.Value.ProcessInstanceKey,
                    Message = createdElement.Value.ErrorMessage,
                    ProcessDefinitionKey = createdElement.Value.ProcessDefinitionKey,
                    State = (createdElement, resolvedElement) switch
                    {
                        { resolvedElement: not null } => ProcessIncidenState.RESOLVED,
                        _ => ProcessIncidenState.ACTIVE,
                    },
                    ProcessElementId = createdElement.Value.ElementId,
                };

                return incident;
            })
            .ToArray();

            incidents.AddRange(localIncidents);
            serchAfter = result.Hits.LastOrDefault()?.Sorts.ToArray() ?? Array.Empty<object>();

            var total = result.HitsMetadata.Total.Value;
            if (incidents.Count < query?.Size!.Value)
            {
                break;
            }
        }

        var incidentDtos = query?.Filter?.State is null
            ? incidents.ToArray()
            : incidents.Where(x => x.State == query!.Filter!.State!).ToArray();

        return new SearchProcessIncidentsQueryResponse(incidentDtos, serchAfter);
    }
}