using Common.Application.Zeebe;
using Common.Zeebe;
using MediatR;

namespace Applications.Application.UseCases.CloseApplication;

[ZeebeTask(Type = "close-application", MaxJobsToActivate = 10, PollingTimeoutInMs = 15_000, PollIntervalInMs = 500, RetryBackOffInMs = new[] { 5_000, 15_000 })]
public record CloseApplicationCommand : IZeebeTask, IRequest
{
    public IJob Job { get; set; }
}
