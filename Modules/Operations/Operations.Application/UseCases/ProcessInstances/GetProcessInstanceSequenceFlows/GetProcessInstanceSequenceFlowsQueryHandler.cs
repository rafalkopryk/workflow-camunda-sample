using CSharpFunctionalExtensions;
using Elastic.Clients.Elasticsearch;
using MediatR;

public record GetProcessInstanceSequenceFlowsQuery(long ProcessInstanceKey) : IRequest<Result<GetProcessInstanceSequenceFlowsQueryResponse>>;

public record GetProcessInstanceSequenceFlowsQueryResponse(string[] Items);

internal class GetProcessInstanceSequenceFlowsQueryHandler : IRequestHandler<GetProcessInstanceSequenceFlowsQuery, Result<GetProcessInstanceSequenceFlowsQueryResponse>>
{
    private readonly ElasticsearchClient _elasticsearchClient;

    public GetProcessInstanceSequenceFlowsQueryHandler(ElasticsearchClient elasticsearchClient)
    {
        _elasticsearchClient = elasticsearchClient;
    }

    public async Task<Result<GetProcessInstanceSequenceFlowsQueryResponse>> Handle(GetProcessInstanceSequenceFlowsQuery request, CancellationToken cancellationToken)
    {
        var luceneSyntax = new QueryLuceneBuilder()
                .Append("value.processInstanceKey", request.ProcessInstanceKey)
                .Append("value.bpmnElementType", "SEQUENCE_FLOW")
                .Build();

        var result = await _elasticsearchClient.SearchAsync<PorcessInstanceDocument>(s => s
            .Index("zeebe-record-process-instance")
            .QueryLuceneSyntax(luceneSyntax));

        //.Query(q => q
        //    .Bool(b => b.Must(m => m
        //        .Term(x => x.Value.ProcessInstanceKey, long.Parse(processInstance))
        //        .Term(x => x.Intent, "SEQUENCE_FLOW_TAKEN")))
        //    ));

        var sequenceFlows = result.Documents.Select(x => x.Value.ElementId).ToArray();
        return new GetProcessInstanceSequenceFlowsQueryResponse(sequenceFlows);
    }
}
