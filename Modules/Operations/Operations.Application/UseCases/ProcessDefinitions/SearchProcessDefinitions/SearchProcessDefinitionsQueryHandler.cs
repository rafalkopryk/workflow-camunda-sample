using CSharpFunctionalExtensions;
using Elastic.Clients.Elasticsearch;
using MediatR;
using Operations.Application.UseCases.ProcessDefinitions.GetProcessDefinitionXml;
using Operations.Application.UseCases.ProcessDefinitions.SearchProcessDefinitions;
using Operations.Application.UseCases.ProcessDefinitions.Shared.Documents;

namespace Operations.Application.UseCases.ProcessDefinitions.SearchProcessDefinition;

internal class SearchProcessDefinitionsQueryHandler : IRequestHandler<SearchProcessDefinitionsQuery, Result<SearchProcessDefinitionsQueryResponse>>
{
    private readonly ElasticsearchClient _elasticsearchClient;

    public SearchProcessDefinitionsQueryHandler(ElasticsearchClient elasticsearchClient)
    {
        _elasticsearchClient = elasticsearchClient;
    }

    public async Task<Result<SearchProcessDefinitionsQueryResponse>> Handle(SearchProcessDefinitionsQuery query, CancellationToken cancellationToken)
    {
        var luceneSyntax = new QueryLuceneBuilder()
            .Append("intent", "CREATED")
            .Append("valueType", "PROCESS")
            .Append("value.bpmnProcessId", query.Filter?.BpmnProcessId)
            .Build();

        var result = await _elasticsearchClient.SearchAsync<ProcessDefinitionDocument>(s => s
            .Index("zeebe-record-process*")
            .Size(999)
            .QueryLuceneSyntax(luceneSyntax));

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