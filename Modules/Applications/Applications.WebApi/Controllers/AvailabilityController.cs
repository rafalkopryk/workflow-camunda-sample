using Applications.Application.UseCases.GetProcessAvailability;
using Applications.Application.UseCases.RegisterApplication;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Applications.WebApi.Controllers;

[ApiController]
[Route("availabilities")]
public class AvailabilityController : ControllerBase
{
    private readonly IMediator _mediator;

    public AvailabilityController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet(Name = "GetProcessAvailability")]
    [ProducesResponseType(typeof(GetProcessAvailabilityQueryResponse),StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProcessAvailability(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProcessAvailabilityQuery(), cancellationToken);
        return Ok(result);
    }
}