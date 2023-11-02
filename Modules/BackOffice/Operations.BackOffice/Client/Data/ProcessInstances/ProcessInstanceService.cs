using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Operations.BackOffice.Client.Data.ProcessInstances.Dto;

namespace Operations.BackOffice.Client.Data.ProcessInstances
{
    internal class ProcessInstanceService : IProcessInstanceService
    {
        private readonly HttpClient _httpClient;
        private readonly ExternalServicesOptions _options;
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        public ProcessInstanceService(HttpClient httpClient, IOptions<ExternalServicesOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<ProcessInstanceDto[]> GetProcessInstances(string bpmnProcessId)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_options.Operations.Url}/process-instances/search",
                new SearchProcessInstanceQuery(new ProcessInstanceDto
                {
                    BpmnProcessId = bpmnProcessId,
                }),
                options: jsonSerializerOptions);

            var queryResponse = await response.Content.ReadFromJsonAsync<SearchProcessInstanceQueryResponse>(options: jsonSerializerOptions);

            return queryResponse.Items;
        }

        public async Task<ProcessInstanceDto[]> GetProcessInstances(long processDefinitionKey)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_options.Operations.Url}/process-instances/search",
                new SearchProcessInstanceQuery(new ProcessInstanceDto
                {
                    ProcessDefinitionKey = processDefinitionKey,
                }),
                options: jsonSerializerOptions);

            var queryResponse = await response.Content.ReadFromJsonAsync<SearchProcessInstanceQueryResponse>(options: jsonSerializerOptions);

            return queryResponse.Items;
        }


        public async Task<string[]> GetProcessInstanceSequenceFlows(long processInstanceKey)
        {
            var sequenceFlows = await _httpClient.GetFromJsonAsync<GetProcessInstanceSequenceFlowsQueryResponse>(
                $"{_options.Operations.Url}/process-instances/{processInstanceKey}/sequence-flows",
                options: jsonSerializerOptions);
            return sequenceFlows.Items.ToArray();
        }

        public async Task<ProcessInstanceDto> GetProcessInstance(long processInstanceKey)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_options.Operations.Url}/process-instances/search",
                new SearchProcessInstanceQuery(new ProcessInstanceDto
                {
                    Key = processInstanceKey
                }),
                options: jsonSerializerOptions);

            var queryResponse = await response.Content.ReadFromJsonAsync<SearchProcessInstanceQueryResponse>(options: jsonSerializerOptions);

            return queryResponse.Items.FirstOrDefault();
        }
    }
}
