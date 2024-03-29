using MediatR;
using Microsoft.AspNetCore.Mvc;
using Operations.Application.UseCases.ProcessDefinitions.GetProcessDefinitionXml;
using Common.Application.Envelope;
using CSharpFunctionalExtensions;
using Operations.Application.UseCases.ProcessDefinitions.SearchProcessDefinitions;

namespace Operations.WebApi.Controllers;

[ApiController]
[Route("process-definitions")]
public class ProcessDefinitionController : BaseController
{
    private readonly IMediator _mediator;
    public ProcessDefinitionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{processDefinitionKey}/xml", Name = "GetProcessDefinitionXml")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProcessDefinitionsXml([FromRoute] long processDefinitionKey, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProcessDefinitionXmlQuery(processDefinitionKey));
        return result.Match(success => Ok(success), failure => Failure(failure));
    }

    [HttpPost("search", Name = "SearchProcessDefinitions")]
    [ProducesResponseType(typeof(SearchProcessDefinitionsQueryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProcessDefinitions([FromBody] SearchProcessDefinitionsQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query);
        return result.Match(success => Ok(success), failure => Failure(failure));
    }
}

