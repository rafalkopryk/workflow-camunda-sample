using MediatR;
using Microsoft.AspNetCore.Mvc;
using Operations.Application.UseCases.ProcessDefinitions.GetProcessDefinitionXml;
using Operations.Application.UseCases.ProcessDefinitions.SearchProcessDefinitions;

namespace Operations.WebApi.Controllers;

[ApiController]
[Route("process-definitions")]
public class ProcessDefinitionController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProcessDefinitionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{processDefinitionKey}/xml", Name = "GetProcessDefinitionXml")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProcessDefinitionsXml([FromRoute] long processDefinitionKey, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProcessDefinitionXmlQuery(processDefinitionKey));
        return Ok(result);
    }

    [HttpPost("search", Name = "SearchProcessDefinitions")]
    [ProducesResponseType(typeof(SearchProcessDefinitionsQueryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProcessDefinitions([FromBody] SearchProcessDefinitionsQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

