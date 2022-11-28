using Common.Application.Zeebe;
using Common.Zeebe;
using MediatR;

namespace Calculations.Application.UseCases.SimulateCreditCommand;

[ZeebeTask(Type = "simulate-credit", MaxJobsToActivate = 10, PollingTimeoutInMs = 15_000, PollIntervalInMs = 500, RetryBackOffInMs = new[] { 1_000, 5_000 })]
public record SimulateCreditCommand : IZeebeTask, IRequest
{
    public IJob Job { get; set; }
}
