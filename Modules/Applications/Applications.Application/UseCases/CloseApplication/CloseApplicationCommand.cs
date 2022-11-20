using Common.Application.Zeebe;
using Common.Zeebe;
using MediatR;

namespace Applications.Application.UseCases.CloseApplication;

[ZeebeTask(Type = "close-application", MaxJobsToActivate = 5, PollingTimeoutInMs = 10_000, PollIntervalInMs = 500)]
public record CloseApplicationCommand : IZeebeTask, IRequest
{
    public IJob Job { get; set; }
}
