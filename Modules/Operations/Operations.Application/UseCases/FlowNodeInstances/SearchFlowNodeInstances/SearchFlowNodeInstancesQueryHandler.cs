using MediatR;
using Nest;
using Operations.Application.UseCases.FlowNodeInstances.Shared.Dto;

namespace Operations.Application.UseCases.FlowNodeInstances.SearchFlowNodeInstances;

internal class SearchFlowNodeInstancesQueryHandler : IRequestHandler<SearchFlowNodeInstancesQuery, SearchFlowNodeInstancesQueryResponse>
{
    private readonly ElasticClient _elasticsearchClient;

    public SearchFlowNodeInstancesQueryHandler(ElasticClient elasticsearchClient)
    {
        _elasticsearchClient = elasticsearchClient;
    }

    public async Task<SearchFlowNodeInstancesQueryResponse> Handle(SearchFlowNodeInstancesQuery query, CancellationToken cancellationToken)
    {
        var result = await _elasticsearchClient.SearchAsync<FlowNodeInstanceDocument>(s => s
            .Index(FlowNodeInstanceKeyword.INDEX)
            .Sort(s => s
                .Ascending(_ => _.Key)
                .Descending(_ => _.Timestamp))
            .Size(10000)
                .Query(q => q
                 .Term(x => x.Value.ProcessInstanceKey, query.Filter.ProcessInstanceKey) && q
                 .Term(x => x.ValueType, FlowNodeInstanceKeyword.VALUETYPE) && q
                 .Terms(x => x.Field(_ => _.Value.BpmnElementType).Terms(FlowNodeInstanceKeyword.FLOW_NODE_BPMN_ELEMENT_TYPES)) && q
                 .Terms(x => x.Field(_ => _.Intent).Terms(FlowNodeInstanceKeyword.INTENT_ELEMENT_COMPLETED, FlowNodeInstanceKeyword.INTENT_ELEMENT_TERMINATED, FlowNodeInstanceKeyword.INTENT_ELEMENT_ACTIVATED))));

        var elementsGroupByKey = result.Documents.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToArray());
        var instances = elementsGroupByKey.Select(documents =>
        {
            var activatedElement = documents.Value.FirstOrDefault(x => x.Intent == FlowNodeInstanceKeyword.INTENT_ELEMENT_ACTIVATED);
            var completedElement = documents.Value.FirstOrDefault(x => x.Intent == FlowNodeInstanceKeyword.INTENT_ELEMENT_COMPLETED);
            var terminatedElement = documents.Value.FirstOrDefault(x => x.Intent == FlowNodeInstanceKeyword.INTENT_ELEMENT_TERMINATED);

            var startDate = activatedElement?.Timestamp;
            var endDate = completedElement?.Timestamp ?? terminatedElement?.Timestamp;
            var processDefinitionKey = activatedElement?.Value?.ProcessDefinitionKey ?? 0;
            var processInstanceKey = activatedElement?.Value.ProcessInstanceKey ?? 0;
            var flowNodeId = activatedElement?.Value.ElementId;
            var type = Enum.TryParse<FlowNodeInstanceType>(activatedElement?.Value.BpmnElementType, out var outType)
              ? outType
              : FlowNodeInstanceType.UNKNOWN;

            return new FlowNodeInstanceDto
            {
                Key = documents.Key,
                StartDate = DateTimeOffset.FromUnixTimeMilliseconds(startDate.Value),
                EndDate = endDate is null
                 ? null
                 : DateTimeOffset.FromUnixTimeMilliseconds(endDate.Value),
                State = (completedElement, terminatedElement) switch
                {
                    { completedElement: not null } => FlowNodeInstanceState.COMPLETED,
                    { terminatedElement: not null } => FlowNodeInstanceState.TERMINATED,
                    _ => FlowNodeInstanceState.ACTIVE,
                },
                ProcessDefinitionKey = processDefinitionKey,
                ProcessInstanceKey = processInstanceKey,
                FlowNodeId = flowNodeId,
                Type = type,
            };
        })
        .ToArray();


        return new SearchFlowNodeInstancesQueryResponse(instances);
    }
}
