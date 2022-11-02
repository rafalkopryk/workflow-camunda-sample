using Common.Application.Zeebe;
using Common.Zeebe;
using MediatR;

namespace Applications.Application.UseCases.CloseApplication;

[ZeebeJob(JobType = "close-application", MaxJobsToActivate = 20, PollingTimeoutInMs = 60_000, PollIntervalInMs = 10_000)]
public record CloseApplicationCommand : IZeebeJob, IRequest
{
    public IJob Job { get; set; }
}
