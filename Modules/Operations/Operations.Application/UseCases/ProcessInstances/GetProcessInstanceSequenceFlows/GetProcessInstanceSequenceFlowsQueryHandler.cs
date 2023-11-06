using CSharpFunctionalExtensions;
using MediatR;
using Nest;

public record GetProcessInstanceSequenceFlowsQuery(long ProcessInstanceKey) : MediatR.IRequest<Result<GetProcessInstanceSequenceFlowsQueryResponse>>;

public record GetProcessInstanceSequenceFlowsQueryResponse(string[] Items);

internal class GetProcessInstanceSequenceFlowsQueryHandler : IRequestHandler<GetProcessInstanceSequenceFlowsQuery, Result<GetProcessInstanceSequenceFlowsQueryResponse>>
{
    private readonly ElasticClient _elasticsearchClient;

    public GetProcessInstanceSequenceFlowsQueryHandler(ElasticClient elasticsearchClient)
    {
        _elasticsearchClient = elasticsearchClient;
    }

    public async Task<Result<GetProcessInstanceSequenceFlowsQueryResponse>> Handle(GetProcessInstanceSequenceFlowsQuery request, CancellationToken cancellationToken)
    {
        var result = await _elasticsearchClient.SearchAsync<PorcessInstanceDocument>(s => s
            .Index("zeebe-record-process-instance")
            .Size(1000)
            .Query(q => q
                 .Term(x => x.Value.ProcessInstanceKey, request.ProcessInstanceKey) && q
                 .Term(x => x.Intent, "SEQUENCE_FLOW_TAKEN")));

        var sequenceFlows = result.Documents.Select(x => x.Value.ElementId).ToArray();
        return new GetProcessInstanceSequenceFlowsQueryResponse(sequenceFlows);
    }
}
