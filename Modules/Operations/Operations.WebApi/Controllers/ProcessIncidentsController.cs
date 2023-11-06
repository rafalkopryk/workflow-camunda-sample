using MediatR;
using Microsoft.AspNetCore.Mvc;
using Common.Application.Envelope;
using CSharpFunctionalExtensions;
using Operations.Application.UseCases.Incidents.SearchProcessIncidents;

namespace Operations.WebApi.Controllers;

[ApiController]
[Route("incidents")]
public class ProcessIncidentsController : BaseController
{
    private readonly IMediator _mediator;
    public ProcessIncidentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("search", Name = "SearchProcesIncidents")]
    [ProducesResponseType(typeof(SearchProcessIncidentsQueryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProcesIncidents([FromBody] SearchProcessIncidentsQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query);
        return result.Match(success => Ok(success), failure => Failure(failure));
    }
}

