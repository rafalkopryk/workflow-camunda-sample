using Common.Application.Zeebe;
using Common.Zeebe;
using MediatR;

namespace Applications.Application.UseCases.SetDecision;

[ZeebeTask(Type = "set-decision-data", MaxJobsToActivate = 5, PollingTimeoutInMs = 10_000, PollIntervalInMs = 500)]
public record SetDecisionCommand : IZeebeTask, IRequest
{
    public IJob Job { get; set; }
}
