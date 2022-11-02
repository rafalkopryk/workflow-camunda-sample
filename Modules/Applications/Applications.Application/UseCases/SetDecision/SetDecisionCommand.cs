using Common.Application.Zeebe;
using MediatR;
using Zeebe.Client.Api.Responses;

namespace Applications.Application.UseCases.SetDecision;

[ZeebeJob(JobType = "set-decision-data", MaxJobsActive = 10, PollingTimeoutInMs = 60_000, TimeoutInMs = 60_000)]
public record SetDecisionCommand : IZeebeJob, IRequest
{
    public IJob Job { get; set; }
}
