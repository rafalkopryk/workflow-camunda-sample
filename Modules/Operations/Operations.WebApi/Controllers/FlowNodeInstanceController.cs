using Common.Application.Envelope;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Operations.Application.UseCases.FlowNodeInstances.SearchFlowNodeInstances;


namespace Credit.Front.Server.Controllers
{
    [ApiController]
    [Route("flownode-instances")]
    public class FlowNodeInstanceController : BaseController
    {
        private readonly IMediator _mediator;

        public FlowNodeInstanceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("search", Name = "SearchFlowNodeInstances")]
        [ProducesResponseType(typeof(SearchFlowNodeInstancesQueryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchFlowNodeInstances([FromBody] SearchFlowNodeInstancesQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query);
            return result.Match(Ok, Failure);
        }
    }
}