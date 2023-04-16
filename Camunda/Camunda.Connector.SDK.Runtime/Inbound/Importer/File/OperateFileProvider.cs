using Camunda.Client.Operate;
using System.Text;

namespace Camunda.Connector.SDK.Runtime.Inbound.Importer.File;

internal class OperateFileProvider : IBpmnProvider
{
    private readonly IOperateClient _client;

    public OperateFileProvider(IOperateClient client)
    {
        _client = client;
    }

    public async Task<byte[]> GetBpmn(ProcessDefinition processDefinition)
    {
        try
        {
            var processDefinitions = await _client.V1ProcessDefinitionsSearchAsync(new QueryProcessDefinition
            {
                Filter = new Client.Operate.ProcessDefinition
                {
                    BpmnProcessId = processDefinition.BpmnProcessId,
                },
                Size = 1,
                Sort = new List<Sort>()
            {
                new Sort
                {
                    Field = "version",
                    Order = SortOrder.DESC
                }
            }
            });

            var processDefinitionKey = processDefinitions.Items?.FirstOrDefault(x => x.BpmnProcessId == processDefinition.BpmnProcessId)?.Key;
            if (processDefinitionKey is null)
                return Array.Empty<byte>();

            var xml = await _client.V1ProcessDefinitionsXmlAsync(processDefinitionKey.Value);
            return Encoding.UTF8.GetBytes(xml);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
