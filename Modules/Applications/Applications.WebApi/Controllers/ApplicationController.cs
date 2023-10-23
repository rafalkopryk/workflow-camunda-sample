using Applications.Application.UseCases.CancelApplication;
using Applications.Application.UseCases.GetApplication;
using Applications.Application.UseCases.RegisterApplication;
using Applications.Application.UseCases.SignContract;
using Common.Application.Envelope;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Applications.WebApi.Controllers;

[ApiController]
[Route("applications")]
public class ApplicationController : BaseController
{
    private readonly IMediator _mediator;

    public ApplicationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost(Name = "RegisterApplication")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterApplicationCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.Match(
            () => Created($"/applications/{command.ApplicationId}", null),
            failure => Failure(failure));
    }

    [HttpGet("{applicationId}", Name = "GetApplication")]
    [ProducesResponseType(typeof(GetApplicationQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] string applicationId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetApplicationQuery(applicationId), cancellationToken);
        return result.Match(success => Ok(success), failure => Failure(failure));
    }

    [HttpPost("{applicationId}/sign", Name = "SignContract")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Sign([FromRoute] string applicationId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SignContractCommand(applicationId), cancellationToken);
        return result.Match(() => Ok(), failure => Failure(failure));
    }

    [HttpPost("{applicationId}/cancel", Name = "CancelApplication")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancel([FromRoute] string applicationId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelApplicationCommand(applicationId), cancellationToken);
        return result.Match(() => Ok(), failure => Failure(failure));
    }
}
