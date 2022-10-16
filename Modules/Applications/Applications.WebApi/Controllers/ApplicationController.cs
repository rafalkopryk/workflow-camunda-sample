using Applications.Application.UseCases.GetApplication;
using Applications.Application.UseCases.RegisterApplication;
using Applications.Application.UseCases.SignContract;
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

    [HttpGet("{applicationId}", Name = "GetApplication")]
    [ProducesResponseType(typeof(GetApplicationQueryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromRoute] string applicationId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetApplicationQuery(applicationId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{applicationId}/sign", Name = "SignContract")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Sign([FromRoute] string applicationId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SignContractCommand(applicationId), cancellationToken);
        return Ok();
    }
}
