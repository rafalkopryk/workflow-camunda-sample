using Common.Application.Zeebe;
using MediatR;
using Zeebe.Client.Api.Responses;

namespace Calculations.Application.UseCases.SimulateCreditCommand;

[ZeebeJob(JobType = "simulate-credit", MaxJobsActive = 5, PollIntervalInMs = 60 * 1000)]
public record SimulateCreditCommand : IZeebeJob, IRequest
{
    public IJob Job { get; set; }
}
