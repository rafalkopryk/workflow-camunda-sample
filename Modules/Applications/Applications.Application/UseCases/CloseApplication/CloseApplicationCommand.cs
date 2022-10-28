using Common.Application.Zeebe;
using MediatR;
using Zeebe.Client.Api.Responses;

namespace Applications.Application.UseCases.CloseApplication;

[ZeebeJob(JobType = "close-application", MaxJobsActive = 1, PollingTimeoutInMs = 60_000, PollIntervalInMs = 10_000)]
public record CloseApplicationCommand : IZeebeJob, IRequest
{
    public IJob Job { get; set; }
}
