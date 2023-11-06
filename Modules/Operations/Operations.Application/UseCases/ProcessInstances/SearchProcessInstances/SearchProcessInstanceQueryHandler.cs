using CSharpFunctionalExtensions;
using MediatR;
using Nest;
using Operations.Application.UseCases.ProcessInstances.Shared.Dto;

public record SearchProcessInstanceQuery(ProcessInstanceDto? Filter, int? Size = 50) : MediatR.IRequest<Result<SearchProcessInstanceQueryResponse>>;

public record SearchProcessInstanceQueryResponse(ProcessInstanceDto[] Items);

internal class SearchProcessInstanceQueryHandler : IRequestHandler<SearchProcessInstanceQuery, Result<SearchProcessInstanceQueryResponse>>
{
    private readonly ElasticClient _elasticsearchClient;

    public SearchProcessInstanceQueryHandler(ElasticClient elasticsearchClient)
    {
        _elasticsearchClient = elasticsearchClient;
    }

    public async Task<Result<SearchProcessInstanceQueryResponse>> Handle(SearchProcessInstanceQuery query, CancellationToken cancellationToken)
    {
        var intents = query.Filter?.State switch
        {
            ProcessInstanceState.COMPLETED => new[] { "ELEMENT_COMPLETED" },
            ProcessInstanceState.CANCELED => new[] { "ELEMENT_TERMINATED" },
            _ => new[] { "ELEMENT_TERMINATED", "ELEMENT_COMPLETED", "ELEMENT_ACTIVATED" }
        };

        var scriptLogic = query.Filter?.State switch
        {
            ProcessInstanceState.COMPLETED => "params.completedElementsCount > 0",
            ProcessInstanceState.CANCELED => "params.termiantedElementsCount > 0",
            ProcessInstanceState.ACTIVE => "params.termiantedElementsCount == 0 && params.completedElementsCount == 0",
            _ => "params.activatedElementsCount > 0"
        };

        var data = await _elasticsearchClient.SearchAsync<PorcessInstanceDocument>(s => s
            .Index("zeebe-record-process-instance")
            .Size(0)
            .Sort(sort => sort.Descending("timestamp"))
            .Query(q => +q
                .Term(t => t.Value.BpmnElementType, "PROCESS") && +q
                .Terms(t => t.Field("intent").Terms(intents)) && +q
                .Term(t => t.Value.BpmnProcessId, query.Filter?.BpmnProcessId) && +q
                .Term(t => t.Value.ProcessDefinitionKey, query.Filter?.ProcessDefinitionKey) && +q
                .Term(t => t.Value.ProcessInstanceKey, query.Filter?.Key))
            .Aggregations(a => a
                .Terms("processInstances", t => t
                    .Field(f => f.Value.ProcessInstanceKey)
                    .Size(query.Size * 100)
                    .Order(o => o.KeyDescending())
                    .Aggregations(aggs => aggs
                        .Filter("terminatedElements", f => f.Filter(ff => ff.Term(t => t.Intent, "ELEMENT_TERMINATED")))
                        .Filter("completedElements", f => f.Filter(ff => ff.Term(t => t.Intent, "ELEMENT_COMPLETED")))
                        .Filter("activatedElements", f => f.Filter(ff => ff.Term(t => t.Intent, "ELEMENT_ACTIVATED")))
                        .BucketSelector("min_bucket_selector", bs => bs
                            .BucketsPath(bp => bp
                                .Add("completedElementsCount", "completedElements._count")
                                .Add("termiantedElementsCount", "terminatedElements._count")
                                .Add("activatedElementsCount", "activatedElements._count"))
                            .Script(scriptLogic))
                        ))));

        var processes = data.Aggregations.Terms("processInstances").Buckets.Select(x => new Element(
                    long.Parse(x.Key),
                    x.Filter("activatedElements")?.DocCount ?? 0,
                    x.Filter("terminatedElements")?.DocCount ?? 0,
                    x.Filter("completedElements")?.DocCount ?? 0))
                .ToArray()
                .Take(query.Size ?? 50);

        var processIds = processes.Select(x => x.Key).ToArray();
        var result = await _elasticsearchClient.SearchAsync<PorcessInstanceDocument>(s => s
           .Index("zeebe-record-process-instance")
           .Size((query.Size ?? 50) * 10)
           .Sort(sort => sort.Descending("timestamp"))
           .Query(q => +q
               .Term(t => t.Value.BpmnElementType, "PROCESS") && +q
               .Terms(t => t.Field("intent").Terms("ELEMENT_TERMINATED", "ELEMENT_COMPLETED", "ELEMENT_ACTIVATED")) && +q
               .Terms(t => t.Field("value.processInstanceKey").Terms(processIds)
               )));

        var documentsPerProcessInstance = result.Documents.GroupBy(x => x.Value.ProcessInstanceKey).ToDictionary(x => x.Key, x => x.ToArray());
        var processInstances = new List<ProcessInstanceDto>();

        foreach (var documents in documentsPerProcessInstance)
        {
            var bpmnProcessId = documents.Value.First().Value.BpmnProcessId;

            var processActivatedElement = documents.Value.FirstOrDefault(x => x.Intent == "ELEMENT_ACTIVATED" && x.Value.BpmnElementType == "PROCESS");
            var processCompletedElement = documents.Value.FirstOrDefault(x => x.Intent == "ELEMENT_COMPLETED" && x.Value.BpmnElementType == "PROCESS");
            var processTerminatedElement = documents.Value.FirstOrDefault(x => x.Intent == "ELEMENT_TERMINATED" && x.Value.BpmnElementType == "PROCESS");

            var startDate = processActivatedElement?.Timestamp;
            var endDate =  processCompletedElement?.Timestamp ?? processTerminatedElement?.Timestamp;
            var version = processActivatedElement?.Value?.Version ?? 0;
            var processDefinitionKey = processActivatedElement?.Value?.ProcessDefinitionKey ?? 0;

            var processInstance = new ProcessInstanceDto
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

            processInstances.Add(processInstance);
        }

        return new SearchProcessInstanceQueryResponse(processInstances.ToArray());
    }
}

public record Element(long Key, long ActivatedElementCount, long TerminatedElementCount, long CompletedElementCount);