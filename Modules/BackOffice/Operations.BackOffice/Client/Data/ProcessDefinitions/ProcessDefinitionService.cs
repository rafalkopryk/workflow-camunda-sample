using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Operations.BackOffice.Client.Data.ProcessDefinitions.Dto;

namespace Operations.BackOffice.Client.Data.ProcessDefinitions
{
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
