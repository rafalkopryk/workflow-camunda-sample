using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Operations.BackOffice.Client.Data.Incidents;
using Operations.BackOffice.Client.Data.Incidents.Dto;

namespace Operations.BackOffice.Client.Data.ProcessDefinitions;
internal class ProcessIncidentService : IProcessIncidentService
{
    private readonly HttpClient _httpClient;

    private readonly ExternalServicesOptions _options;

    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public ProcessIncidentService(HttpClient httpClient, IOptions<ExternalServicesOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<SearchProcessIncidentsQueryResponse> SearchProcessIncident(SearchProcessIncidentsQuery searchProcessIncidentsQuery)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"{_options.Operations.Url}/incidents/search",
            searchProcessIncidentsQuery,
            options: jsonSerializerOptions);

        var queryResponse = await response.Content.ReadFromJsonAsync<SearchProcessIncidentsQueryResponse>(options: jsonSerializerOptions);

        return queryResponse;
    }
}
