using MediatR;
using Nest;
using Operations.Application.Incidents.SearchProcessIncidents.Shared.Documents;
using Operations.Application.UseCases.Incidents.Shared;
using Operations.Application.UseCases.ProcessDefinitions.GetProcessDefinitionXml;

namespace Operations.Application.UseCases.Incidents.SearchProcessIncidents;

internal class SearchProcessIncidentsQueryHandler : IRequestHandler<SearchProcessIncidentsQuery, SearchProcessIncidentsQueryResponse>
{
    private readonly ElasticClient _elasticsearchClient;

    public SearchProcessIncidentsQueryHandler(ElasticClient elasticsearchClient)
    {
        _elasticsearchClient = elasticsearchClient;
    }

    public async Task<SearchProcessIncidentsQueryResponse> Handle(SearchProcessIncidentsQuery query, CancellationToken cancellationToken)
    {
        var intentsQuery = query.Filter?.State switch
        {
            ProcessIncidenState.RESOLVED => [ProcessIncidentKeyword.INTENT_RESOLVED],
            _ => new[] { ProcessIncidentKeyword.INTENT_CREATED, ProcessIncidentKeyword.INTENT_RESOLVED }
        };

        var intentsFilter = query.Filter?.State switch
        {
            ProcessIncidenState.ACTIVE => [ProcessIncidentKeyword.INTENT_CREATED],
            ProcessIncidenState.RESOLVED => [ProcessIncidentKeyword.INTENT_RESOLVED],
            _ => new[] { ProcessIncidentKeyword.INTENT_CREATED, ProcessIncidentKeyword.INTENT_RESOLVED }
        };

        var searchAfter = query.SearchAfter?.Cast<object>().ToArray();
        var keys = new List<long>();
        while (true)
        {
            var data = await _elasticsearchClient.SearchAsync<ProcessIncidentDocument>(s => s
                .Index(ProcessIncidentKeyword.INDEX)
                .Size(1000)
                .Sort(sort => sort
                    .Descending(_ => _.Timestamp)
                    .Descending(_ => _.Key))
                .SearchAfter(searchAfter)
                .Query(q => +q
                     .Term(x => x.ValueType, ProcessIncidentKeyword.VALUETYPE) && +q
                     .Terms(x => x.Field(_ => _.Intent).Terms(intentsQuery)) && +q
                     .Term(x => x.Value.ProcessDefinitionKey, query.Filter?.ProcessDefinitionKey) && +q
                     .Term(x => x.Value.ProcessInstanceKey, query.Filter?.ProcessInstanceKey) && +q
                     .Term(x => x.Key, query.Filter?.Key)));

            var sizeToTake = query.Size!.Value - keys.Count;
            var keysToTake = data.Documents.GroupBy(x => x.Key).Select(x => x.First()).Where(x => intentsFilter.Contains(x.Intent)).Take(sizeToTake).Select(x => x.Key).ToArray();
            var hitsToTake = data.Hits.GroupBy(x => x.Source.Key).Select(x => x.First()).Where(x => intentsFilter.Contains(x.Source.Intent)).Take(sizeToTake).ToArray();

            keys.AddRange(keysToTake);

            searchAfter = hitsToTake.LastOrDefault()?.Sorts.ToArray() ?? data.Hits?.LastOrDefault()?.Sorts.ToArray() ?? Array.Empty<object>();

            if (keys.Count >= query.Size!.Value || searchAfter?.Length == 0)
            {
                break;
            }
        }

        if (!keys.Any())
        {
            return new SearchProcessIncidentsQueryResponse(Array.Empty<ProcessIncidentDto>(), searchAfter.Cast<object>().ToArray());
        }


        var result = await _elasticsearchClient.SearchAsync<ProcessIncidentDocument>(s => s
           .Index(ProcessIncidentKeyword.INDEX)
           .Size(query.Size! * 3)
           .Sort(sort => sort.Descending(_ => _.Timestamp))
           .Query(q => +q
               .Term(x => x.ValueType, ProcessIncidentKeyword.VALUETYPE) && +q
               .Terms(t => t.Field(_ => _.Intent).Terms(ProcessIncidentKeyword.INTENT_CREATED, ProcessIncidentKeyword.INTENT_RESOLVED)) && +q
               .Terms(t => t.Field(_ => _.Key).Terms(keys))));

        var incidentElementsGroupByKey = result.Documents.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToArray());
        var incidents = incidentElementsGroupByKey.Select(documents =>
        {
            var createdElement = documents.Value.FirstOrDefault(x => x.Intent == ProcessIncidentKeyword.INTENT_CREATED);
            var resolvedElement = documents.Value.FirstOrDefault(x => x.Intent == ProcessIncidentKeyword.INTENT_RESOLVED);

            var incident = new ProcessIncidentDto
            {
                Key = documents.Key,
                CreationTime = DateTimeOffset.FromUnixTimeMilliseconds(createdElement!.Timestamp),
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

        return new SearchProcessIncidentsQueryResponse(incidents, searchAfter);
    }
}
