using Azure.Core;
using CSharpFunctionalExtensions;
using MediatR;
using Nest;
using Operations.Application.UseCases.ProcessDefinitions.GetProcessDefinitionXml;
using Operations.Application.UseCases.ProcessDefinitions.SearchProcessDefinitions;
using Operations.Application.UseCases.ProcessDefinitions.Shared.Documents;

namespace Operations.Application.UseCases.ProcessDefinitions.SearchProcessDefinition;

internal class SearchProcessDefinitionsQueryHandler : IRequestHandler<SearchProcessDefinitionsQuery, Result<SearchProcessDefinitionsQueryResponse>>
{
    private readonly ElasticClient _elasticsearchClient;

    public SearchProcessDefinitionsQueryHandler(ElasticClient elasticsearchClient)
    {
        _elasticsearchClient = elasticsearchClient;
    }

    public async Task<Result<SearchProcessDefinitionsQueryResponse>> Handle(SearchProcessDefinitionsQuery query, CancellationToken cancellationToken)
    {
        var result = await _elasticsearchClient.SearchAsync<ProcessDefinitionDocument>(s => s
            .Index("zeebe-record-process*")
            .Size(10000)
            .Sort(sort => sort
                .Descending("timestamp")
                .Descending(_ => _.Value.Version))
            .Query(q => +q   
                 .Term(x => x.ValueType, "PROCESS") && +q
                 .Term(x => x.Intent, "CREATED") && +q
                 .Term(x => x.Value.BpmnProcessId, query.Filter?.BpmnProcessId) && +q
                 .Term(x => x.Value.ProcessDefinitionKey, query.Filter?.Key) && +q
                 .Term(x => x.Value.Version, query.Filter?.Version)));

        var processDefinitions = result.Documents.Select(document =>
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

        return new SearchProcessDefinitionsQueryResponse(processDefinitions);
    }
}