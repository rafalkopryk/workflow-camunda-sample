using Common.Application.Zeebe;
using Common.Zeebe;
using MediatR;

namespace Calculations.Application.UseCases.SimulateCreditCommand;

[ZeebeTask(Type = "simulate-credit", MaxJobsToActivate = 5, PollingTimeoutInMs = 20_000, PollIntervalInMs = 500)]
public record SimulateCreditCommand : IZeebeTask, IRequest
{
    public IJob Job { get; set; }
}
