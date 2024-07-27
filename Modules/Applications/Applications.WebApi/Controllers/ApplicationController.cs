using Applications.Application.UseCases.CancelApplication;
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterApplicationCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result switch
        {
            RegisterApplicationCommandResponse.OK => Created($"/applications/{command.ApplicationId}", null),
            RegisterApplicationCommandResponse.ResourceExists resourceExists => Problem(statusCode: StatusCodes.Status422UnprocessableEntity, title: nameof(resourceExists)),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
    }

    [HttpGet("{applicationId}", Name = "GetApplication")]
    [ProducesResponseType(typeof(GetApplicationQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] string applicationId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetApplicationQuery(applicationId), cancellationToken);
        return result switch
        {
            GetApplicationQueryResponse.OK ok => Ok(ok.CreditApplication),
            GetApplicationQueryResponse.ResourceNotFound resourceNotFound => Problem(statusCode: StatusCodes.Status404NotFound, title: nameof(resourceNotFound)),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
    }

    [HttpPost("{applicationId}/sign", Name = "SignContract")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Sign([FromRoute] string applicationId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SignContractCommand(applicationId), cancellationToken);
        return result switch
        {
            SignContractCommandResponse.OK => Ok(),
            SignContractCommandResponse.ResourceNotFound resourceNotFound => Problem(statusCode: StatusCodes.Status404NotFound, title: nameof(resourceNotFound)),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
    }

    [HttpPost("{applicationId}/cancel", Name = "CancelApplication")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancel([FromRoute] string applicationId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelApplicationCommand(applicationId), cancellationToken);
        return result switch
        {
            CancelApplicationCommandResponse.OK => Ok(),
            CancelApplicationCommandResponse.ResourceNotFound resourceNotFound => Problem(statusCode: StatusCodes.Status404NotFound, title: nameof(resourceNotFound)),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
    }
}
