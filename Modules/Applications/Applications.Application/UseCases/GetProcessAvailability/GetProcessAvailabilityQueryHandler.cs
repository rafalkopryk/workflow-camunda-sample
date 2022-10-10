using Common.Application.Zeebe;
using MediatR;

namespace Applications.Application.UseCases.GetProcessAvailability;

internal class GetProcessAvailabilityQueryHandler : IRequestHandler<GetProcessAvailabilityQuery, GetProcessAvailabilityQueryResponse>
{
    private readonly IZeebeService _zeebeService;

    public GetProcessAvailabilityQueryHandler(IZeebeService zeebeService)
    {
        _zeebeService = zeebeService;
    }

    public async Task<GetProcessAvailabilityQueryResponse> Handle(GetProcessAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var result = await _zeebeService.Status();
        return new()
        {
            Status = result.ToString()
        };
    }
}
