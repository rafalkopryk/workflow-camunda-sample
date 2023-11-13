using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Operations.BackOffice.Client.Data.ProcessFlowNodeInstances.Dto;

namespace Operations.BackOffice.Client.Data.ProcessFlowNodeInstances
{
    internal class ProcessFlowNodeInstanceService : IProcessFlowNodeInstanceService
    {
        private readonly HttpClient _httpClient;
        private readonly ExternalServicesOptions _options;
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        public ProcessFlowNodeInstanceService(HttpClient httpClient, IOptions<ExternalServicesOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<SearchFlowNodeInstancesQueryResponse> SearchProcessFlowNodeInstance(SearchFlowNodeInstancesQuery? query)
        {
            var response = await _httpClient.PostAsJsonAsync(
               $"{_options.Operations.Url}/flownode-instances/search",
               query,
               options: jsonSerializerOptions);

            var queryResponse = await response.Content.ReadFromJsonAsync<SearchFlowNodeInstancesQueryResponse>(options: jsonSerializerOptions);

            return queryResponse;
        }
    }
}
