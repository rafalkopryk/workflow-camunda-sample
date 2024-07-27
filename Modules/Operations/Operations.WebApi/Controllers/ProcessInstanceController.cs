using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Credit.Front.Server.Controllers
{
    [ApiController]
    [Route("process-instances")]
    public class ProcessInstanceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProcessInstanceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{key}/sequence-flows", Name = "GetProcessInstanceSequenceFlows")]
        [ProducesResponseType(typeof(GetProcessInstanceSequenceFlowsQueryResponse),StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSequenceFlows([FromRoute] long key, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProcessInstanceSequenceFlowsQuery(key));
            return Ok(result); 
        }

        [HttpPost("search", Name = "SearchProcessInstances")]
        [ProducesResponseType(typeof(SearchProcessInstanceQueryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchProcessInstances([FromBody] SearchProcessInstanceQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
