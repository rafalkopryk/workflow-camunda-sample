using Common.Application.Zeebe;
using Common.Zeebe;
using MediatR;

namespace Applications.Application.UseCases.CloseApplication;

[ZeebeTask(Type = "close-application", MaxJobsToActivate = 5, PollingTimeoutInMs = 20_000, PollIntervalInMs = 5_000)]
public record CloseApplicationCommand : IZeebeTask, IRequest
{
    public IJob Job { get; set; }
}
