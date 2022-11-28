using Common.Application.Zeebe;
using Common.Zeebe;
using MediatR;

namespace Applications.Application.UseCases.SetDecision;

[ZeebeTask(Type = "set-decision-data", MaxJobsToActivate = 10, PollingTimeoutInMs = 15_000, PollIntervalInMs = 500, RetryBackOffInMs = new[] { 1_000, 5_000 })]
public record SetDecisionCommand : IZeebeTask, IRequest
{
    public IJob Job { get; set; }
}
