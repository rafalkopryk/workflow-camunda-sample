using CSharpFunctionalExtensions;
using MediatR;
using Nest;
using Operations.Application.UseCases.ProcessDefinitions.Shared;
using Operations.Application.UseCases.ProcessDefinitions.Shared.Documents;
using System.Text;

namespace Operations.Application.UseCases.ProcessDefinitions.GetProcessDefinitionXml;

internal class GetProcessDefinitionXmlQueryHandler : IRequestHandler<GetProcessDefinitionXmlQuery, Result<string>>
{
    private readonly ElasticClient _elasticsearchClient;

    public GetProcessDefinitionXmlQueryHandler(ElasticClient elasticsearchClient)
    {
        _elasticsearchClient = elasticsearchClient;
    }

    public async Task<Result<string>> Handle(GetProcessDefinitionXmlQuery request, CancellationToken cancellationToken)
    {
        var result = await _elasticsearchClient.SearchAsync<ProcessDefinitionDocument>(s => s
            .Size(1)
            .Index(ProcessDefinitionKeyword.INDEX)
            .Query(q => q
                    .Term(x => x.ValueType, ProcessDefinitionKeyword.VALUETYPE) && q
                    .Term(x => x.Intent, ProcessDefinitionKeyword.INTENT_CREATED) && q
                    .Term(x => x.Value.ProcessDefinitionKey, request.ProcessDefinitionKey)));

        var xmlBase64 = result.Documents.FirstOrDefault()?.Value?.Resource;
        var xmlString = Encoding.UTF8.GetString(Convert.FromBase64String(xmlBase64));
        return xmlString;
    }
}
