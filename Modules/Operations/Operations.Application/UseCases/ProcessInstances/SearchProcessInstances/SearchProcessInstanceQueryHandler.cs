using CSharpFunctionalExtensions;
using MediatR;
using Nest;
using Operations.Application.UseCases.ProcessInstances.Shared.Dto;

public record SearchProcessInstanceQuery(ProcessInstanceDto? Filter, long[]? SearchAfter, int? Size = 50) : MediatR.IRequest<Result<SearchProcessInstanceQueryResponse>>;

public record SearchProcessInstanceQueryResponse(ProcessInstanceDto[] Items, object[] SortValues);

internal class SearchProcessInstanceQueryHandler : IRequestHandler<SearchProcessInstanceQuery, Result<SearchProcessInstanceQueryResponse>>
{
    private readonly ElasticClient _elasticsearchClient;



    public SearchProcessInstanceQueryHandler(ElasticClient elasticsearchClient)
    {
        _elasticsearchClient = elasticsearchClient;
    }

    public async Task<Result<SearchProcessInstanceQueryResponse>> Handle(SearchProcessInstanceQuery query, CancellationToken cancellationToken)
    {
        var intentsQuery = query.Filter?.State switch
        {
            ProcessInstanceState.COMPLETED => new[] { ProcessInstanceKeyword.INTENT_ELEMENT_COMPLETED },
            ProcessInstanceState.CANCELED => new[] { ProcessInstanceKeyword.INTENT_ELEMENT_TERMINATED },
            _ => new[] { ProcessInstanceKeyword.INTENT_ELEMENT_TERMINATED, ProcessInstanceKeyword.INTENT_ELEMENT_COMPLETED, ProcessInstanceKeyword.INTENT_ELEMENT_ACTIVATED }
        };

        var intentsFilter = query.Filter?.State switch
        {
            ProcessInstanceState.COMPLETED => new[] { ProcessInstanceKeyword.INTENT_ELEMENT_COMPLETED },
            ProcessInstanceState.CANCELED => new[] { ProcessInstanceKeyword.INTENT_ELEMENT_TERMINATED },
            ProcessInstanceState.ACTIVE => new[] { ProcessInstanceKeyword.INTENT_ELEMENT_ACTIVATED },
            _ => new[] { ProcessInstanceKeyword.INTENT_ELEMENT_TERMINATED, ProcessInstanceKeyword.INTENT_ELEMENT_COMPLETED, ProcessInstanceKeyword.INTENT_ELEMENT_ACTIVATED }
        };

        var processInstancesKeys = new List<long>();
        var searchAfter = query.SearchAfter?.Cast<object>().ToArray();
        while (true)
        {
            var data = await _elasticsearchClient.SearchAsync<PorcessInstanceDocument>(s => s
                .Index(ProcessInstanceKeyword.INDEX)
                .Size(10000)
                .Sort(sort => sort
                    .Descending(_ => _.Value.ProcessInstanceKey)
                    .Descending(_ => _.Timestamp))
                .SearchAfter(searchAfter)
                .Query(q => +q
                    .Term(t => t.Value.BpmnElementType, ProcessInstanceKeyword.BPMN_ELEMENT_TYPE) && +q
                    .Terms(t => t.Field(_ => _.Intent).Terms(intentsQuery)) && +q
                    .Term(t => t.Value.BpmnProcessId, query.Filter?.BpmnProcessId) && +q
                    .Term(t => t.Value.ProcessDefinitionKey, query.Filter?.ProcessDefinitionKey) && +q
                    .Term(t => t.Value.ProcessInstanceKey, query.Filter?.Key)));

            var sizeToTake = query.Size!.Value - processInstancesKeys.Count;
            var processInstancesKeysToTake = data.Documents.GroupBy(x => x.Value.ProcessInstanceKey).Select(x => x.First()).Where(x => intentsFilter.Contains(x.Intent)).Take(sizeToTake).Select(x => x.Value.ProcessInstanceKey).ToArray();
            var hitsToTake = data.Hits.GroupBy(x => x.Source.Value.ProcessInstanceKey).Select(x => x.First()).Where(x => intentsFilter.Contains(x.Source.Intent)).Take(sizeToTake).ToArray();

            processInstancesKeys.AddRange(processInstancesKeysToTake);

            searchAfter = hitsToTake.LastOrDefault()?.Sorts.ToArray() ?? data.Hits?.LastOrDefault()?.Sorts.ToArray() ?? Array.Empty<object>();

            if (processInstancesKeys.Count >= query.Size!.Value || searchAfter?.Length == 0)
            {
                break;
            }
        }

        if (!processInstancesKeys.Any())
        {
            return new SearchProcessInstanceQueryResponse(Array.Empty<ProcessInstanceDto>(), searchAfter.Cast<object>().ToArray());
        }

        var result = await _elasticsearchClient.SearchAsync<PorcessInstanceDocument>(s => s
           .Index(ProcessInstanceKeyword.INDEX)
           .Size(query.Size! * 3)
           .Sort(sort => sort.Descending(_ => _.Timestamp))
           .Query(q => +q
               .Term(t => t.Value.BpmnElementType, ProcessInstanceKeyword.BPMN_ELEMENT_TYPE) && +q
               .Terms(t => t.Field(_ => _.Intent).Terms(ProcessInstanceKeyword.INTENT_ELEMENT_TERMINATED, ProcessInstanceKeyword.INTENT_ELEMENT_COMPLETED, ProcessInstanceKeyword.INTENT_ELEMENT_ACTIVATED)) && +q
               .Terms(t => t.Field(_ => _.Value.ProcessInstanceKey).Terms(processInstancesKeys))));

        var documentsPerProcessInstance = result.Documents.GroupBy(x => x.Value.ProcessInstanceKey).ToDictionary(x => x.Key, x => x.ToArray());

        var processInstances = documentsPerProcessInstance.Select(documents =>
        {
            var bpmnProcessId = documents.Value.First().Value.BpmnProcessId;

            var processActivatedElement = documents.Value.FirstOrDefault(x => x.Intent == ProcessInstanceKeyword.INTENT_ELEMENT_ACTIVATED);
            var processCompletedElement = documents.Value.FirstOrDefault(x => x.Intent == ProcessInstanceKeyword.INTENT_ELEMENT_COMPLETED);
            var processTerminatedElement = documents.Value.FirstOrDefault(x => x.Intent == ProcessInstanceKeyword.INTENT_ELEMENT_TERMINATED);

            var startDate = processActivatedElement?.Timestamp;
            var endDate = processCompletedElement?.Timestamp ?? processTerminatedElement?.Timestamp;
            var version = processActivatedElement?.Value?.Version ?? 0;
            var processDefinitionKey = processActivatedElement?.Value?.ProcessDefinitionKey ?? 0;

            return new ProcessInstanceDto
            {
                Key = documents.Key,
                BpmnProcessId = bpmnProcessId,
                StartDate = startDate is null
                    ? null
                    : DateTimeOffset.FromUnixTimeMilliseconds(startDate.Value),
                EndDate = endDate is null
                 ? null
                 : DateTimeOffset.FromUnixTimeMilliseconds(endDate.Value),
                State = (processCompletedElement, processTerminatedElement) switch
                {
                    { processCompletedElement: not null } => ProcessInstanceState.COMPLETED,
                    { processTerminatedElement: not null } => ProcessInstanceState.CANCELED,
                    _ => ProcessInstanceState.ACTIVE,
                },
                ProcessVersion = version,
                ProcessDefinitionKey = processDefinitionKey,
            };
        })
        .ToArray();

        return new SearchProcessInstanceQueryResponse(processInstances.ToArray(), searchAfter.Cast<object>().ToArray());
    }
}
