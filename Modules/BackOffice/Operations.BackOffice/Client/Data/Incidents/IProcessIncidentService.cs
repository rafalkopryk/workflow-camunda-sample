using Operations.BackOffice.Client.Data.Incidents.Dto;

namespace Operations.BackOffice.Client.Data.Incidents;

public interface IProcessIncidentService
{
    Task<SearchProcessIncidentsQueryResponse> SearchProcessIncident(SearchProcessIncidentsQuery searchProcessIncidentsQuery);
}
