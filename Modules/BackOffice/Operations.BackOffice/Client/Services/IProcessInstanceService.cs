using Microsoft.Extensions.Options;
using Operations.BackOffice.Client.Dto.ProcessDefinitions;
using Operations.BackOffice.Client.Dto.ProcessInstances;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Operations.BackOffice.Client.Services
{
    public interface IProcessDefinitionService
    {
        Task<ProcessDefinitionDto[]> GetProcessDefinitions();

        Task<ProcessDefinitionDto[]> GetProcessDefinitions(string bpmnProcessId);


        Task<string> GetProcessDefinitionXml(long key);
    }

    public interface IProcessInstanceService
    {
        Task<ProcessInstanceDto[]> GetProcessInstances(string bpmnProcessId);

        Task<ProcessInstanceDto> GetProcessInstance(long processInstanceKey);

        Task<string[]> GetProcessInstanceSequenceFlows(long processInstanceKey);
    }


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
                    BpmnProcessId = bpmnProcessId
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

    internal class ProcessDefinitionService : IProcessDefinitionService
    {
        private readonly HttpClient _httpClient;

        private readonly ExternalServicesOptions _options;

        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        public ProcessDefinitionService(HttpClient httpClient, IOptions<ExternalServicesOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<ProcessDefinitionDto[]> GetProcessDefinitions()
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_options.Operations.Url}/process-definitions/search",
                new SearchProcessDefinitionsQuery(null),
                options: jsonSerializerOptions);

            var queryResponse = await response.Content.ReadFromJsonAsync<SearchProcessDefinitionsQueryResponse>(options: jsonSerializerOptions);

            return queryResponse.Items;
        }

        public async Task<string> GetProcessDefinitionXml(long key)
        {
            var response = await _httpClient.GetStringAsync(
                $"{_options.Operations.Url}/process-definitions/{key}/xml");

            return response;
        }

        public async Task<ProcessDefinitionDto[]> GetProcessDefinitions(string bpmnProcessId)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_options.Operations.Url}/process-definitions/search",
                new SearchProcessDefinitionsQuery(new ProcessDefinitionDto
                {
                    BpmnProcessId = bpmnProcessId
                }),
                options: jsonSerializerOptions);

            var queryResponse = await response.Content.ReadFromJsonAsync<SearchProcessDefinitionsQueryResponse>(options: jsonSerializerOptions);

            return queryResponse.Items;
        }
    }
}
