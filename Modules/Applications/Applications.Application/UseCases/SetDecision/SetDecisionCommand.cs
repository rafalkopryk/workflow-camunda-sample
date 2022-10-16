using Common.Application.Zeebe;
using MediatR;
using Zeebe.Client.Api.Responses;

namespace Applications.Application.UseCases.SetDecision;

[ZeebeJob(JobType = "set-decision-data", MaxJobsActive = 5, PollIntervalInMs = 60 * 1000)]
public record SetDecisionCommand : IZeebeJob, IRequest
{
    public IJob Job { get; set; }
}
