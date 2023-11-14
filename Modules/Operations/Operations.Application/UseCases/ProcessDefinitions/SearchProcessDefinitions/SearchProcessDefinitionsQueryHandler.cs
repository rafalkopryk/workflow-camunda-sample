using CSharpFunctionalExtensions;
using MediatR;
using Nest;
using Operations.Application.UseCases.ProcessDefinitions.GetProcessDefinitionXml;
using Operations.Application.UseCases.ProcessDefinitions.SearchProcessDefinitions;
using Operations.Application.UseCases.ProcessDefinitions.Shared;
using Operations.Application.UseCases.ProcessDefinitions.Shared.Documents;

namespace Operations.Application.UseCases.ProcessDefinitions.SearchProcessDefinition;

internal class SearchProcessDefinitionsQueryHandler(ElasticClient elasticsearchClient) : IRequestHandler<SearchProcessDefinitionsQuery, Result<SearchProcessDefinitionsQueryResponse>>
{
    private readonly ElasticClient _elasticsearchClient = elasticsearchClient;
    
    public async Task<Result<SearchProcessDefinitionsQueryResponse>> Handle(SearchProcessDefinitionsQuery query, CancellationToken cancellationToken)
    {
        var searchAfter = query.SearchAfter?.Cast<object>().ToArray();

        var data = await _elasticsearchClient.SearchAsync<ProcessDefinitionDocument>(s => s
            .Index(ProcessDefinitionKeyword.INDEX)
            .Size(query.Size!.Value)
            .Sort(sort => sort
                .Descending(_ => _.Timestamp))
            .Query(q => +q
                 .Term(x => x.ValueType, ProcessDefinitionKeyword.VALUETYPE) && +q
                 .Term(x => x.Intent, ProcessDefinitionKeyword.INTENT_CREATED) && +q
                 .Term(x => x.Value.BpmnProcessId, query.Filter?.BpmnProcessId) && +q
                 .Term(x => x.Value.ProcessDefinitionKey, query.Filter?.Key) && +q
                 .Term(x => x.Value.Version, query.Filter?.Version)));

        searchAfter = data.Hits?.LastOrDefault()?.Sorts.ToArray() ?? Array.Empty<object>();

        var processDefinitions = data.Documents.Select(document =>
        {
            var bpmnProcessId = document.Value.BpmnProcessId;
            return new ProcessDefinitionDto
            {
                Key = document.Value.ProcessDefinitionKey,
                BpmnProcessId = bpmnProcessId,
                Name = document.Value.ResourceName,
                Version = document.Value.Version,
            };
        })
        .ToArray();

        return new SearchProcessDefinitionsQueryResponse(processDefinitions, searchAfter);
    }
}
