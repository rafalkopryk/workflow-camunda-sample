using Applications.Application.UseCases.RegisterApplication;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Applications.WebApi.Controllers;

[ApiController]
[Route("applications")]
public class ApplicationController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpPost(Name = "RegisterApplication")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterApplicationCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Created($"/applications/{command.ApplicationId}", null);
    }
}
