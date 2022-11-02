using Common.Application.Zeebe;
using Common.Zeebe;
using MediatR;

namespace Calculations.Application.UseCases.SimulateCreditCommand;

[ZeebeJob(JobType = "simulate-credit", MaxJobsToActivate = 10, PollingTimeoutInMs = 60_000)]
public record SimulateCreditCommand : IZeebeJob, IRequest
{
    public IJob Job { get; set; }
}
