﻿using CSharpFunctionalExtensions;
using Elastic.Clients.Elasticsearch;
using MediatR;
using Operations.Application.UseCases.ProcessInstances.Shared.Dto;

public record SearchProcessInstanceQuery(ProcessInstanceDto? Filter) : IRequest<Result<SearchProcessInstanceQueryResponse>>;

public record SearchProcessInstanceQueryResponse(ProcessInstanceDto[] Items);


internal class SearchProcessInstanceQueryHandler : IRequestHandler<SearchProcessInstanceQuery, Result<SearchProcessInstanceQueryResponse>>
{
    private readonly ElasticsearchClient _elasticsearchClient;

    public SearchProcessInstanceQueryHandler(ElasticsearchClient elasticsearchClient)
    {
        _elasticsearchClient = elasticsearchClient;
    }

    public async Task<Result<SearchProcessInstanceQueryResponse>> Handle(SearchProcessInstanceQuery query, CancellationToken cancellationToken)
    {
        var luceneSyntax = new QueryLuceneBuilder()
            .Append("value.bpmnProcessId", query.Filter?.BpmnProcessId)
            .Append("value.processInstanceKey", query.Filter?.Key)
            .Append("value.processVersion", query.Filter?.ProcessVersion)
            .Append("value.processDefinitionKey", query.Filter?.ProcessDefinitionKey)
            .Build();

        var result = await _elasticsearchClient.SearchAsync<PorcessInstanceDocument>(s => s
           .Index("zeebe-record-process-instance*")
           .Size(999)
           .QueryLuceneSyntax(luceneSyntax));

        var documentsPerProcessInstance = result.Documents.GroupBy(x => x.Value.ProcessInstanceKey).ToDictionary(x => x.Key, x => x.ToArray());
        var processInstances = new List<ProcessInstanceDto>();

        foreach (var documents in documentsPerProcessInstance)
        {
            var bpmnProcessId = documents.Value.First().Value.BpmnProcessId;

            var startDate = documents.Value.FirstOrDefault(x => x.Intent == "ELEMENT_ACTIVATING" && x.Value.BpmnElementType == "PROCESS")?.Timestamp;
            var endDate = documents.Value.FirstOrDefault(x => x.Intent == "ELEMENT_COMPLETED" && x.Value.BpmnElementType == "PROCESS")?.Timestamp;
            var cancelDate = documents.Value.FirstOrDefault(x => x.Intent == "ELEMENT_CANCALED" && x.Value.BpmnElementType == "PROCESS")?.Timestamp;
            var version = documents.Value.FirstOrDefault(x => x.Intent == "ELEMENT_ACTIVATING" && x.Value.BpmnElementType == "PROCESS")?.Value?.Version ?? 0;
            var processDefinitionKey = documents.Value.FirstOrDefault(x => x.Intent == "ELEMENT_ACTIVATING" && x.Value.BpmnElementType == "PROCESS")?.Value?.ProcessDefinitionKey ?? 0;

            var processInstance = new ProcessInstanceDto
            {
                Key = documents.Key,
                BpmnProcessId = bpmnProcessId,
                StartDate = DateTimeOffset.FromUnixTimeMilliseconds(startDate.Value),
                EndDate = endDate is null
                 ? null
                 : DateTimeOffset.FromUnixTimeMilliseconds(endDate.Value),
                State = (endDate, cancelDate) switch
                {
                    { endDate: not null } => ProcessInstanceState.COMPLETED,
                    { cancelDate: not null } => ProcessInstanceState.CANCELED,
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
