using CSharpFunctionalExtensions;
using Elastic.Clients.Elasticsearch;
using MediatR;
using Operations.Application.UseCases.ProcessDefinitions.Shared.Documents;
using System.Text;

namespace Operations.Application.UseCases.ProcessDefinitions.GetProcessDefinitionXml;

internal class GetProcessDefinitionXmlQueryHandler : IRequestHandler<GetProcessDefinitionXmlQuery, Result<string>>
{
    private readonly ElasticsearchClient _elasticsearchClient;

    public GetProcessDefinitionXmlQueryHandler(ElasticsearchClient elasticsearchClient)
    {
        _elasticsearchClient = elasticsearchClient;
    }

    public async Task<Result<string>> Handle(GetProcessDefinitionXmlQuery request, CancellationToken cancellationToken)
    {
        var luceneSyntax = new QueryLuceneBuilder()
            .Append("intent", "CREATED")
            .Append("valueType", "PROCESS")
            .Append("value.processDefinitionKey", request.ProcessDefinitionKey)
            .Build();

        var result = await _elasticsearchClient.SearchAsync<ProcessDefinitionDocument>(s => s
                .Size(1)
                .Index("zeebe-record-process*")
                    .QueryLuceneSyntax(luceneSyntax));

        var xmlBase64 = result.Documents.FirstOrDefault()?.Value?.Resource;
        var xmlString = Encoding.UTF8.GetString(Convert.FromBase64String(xmlBase64));
        return xmlString;
    }
}
