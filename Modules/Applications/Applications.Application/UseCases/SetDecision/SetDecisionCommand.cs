using Common.Application.Zeebe;
using Common.Zeebe;
using MediatR;

namespace Applications.Application.UseCases.SetDecision;

[ZeebeJob(JobType = "set-decision-data", MaxJobsToActivate = 10, PollingTimeoutInMs = 60_000)]
public record SetDecisionCommand : IZeebeJob, IRequest
{
    public IJob Job { get; set; }
}
