﻿using Common.Application.Zeebe;
using MediatR;
using Zeebe.Client.Api.Responses;

namespace Calculations.Application.UseCases.SimulateCreditCommand;

[ZeebeJob(JobType = "simulate-credit", MaxJobsActive = 5, PollingTimeoutInMs = 60_000, TimeoutInMs = 60_000)]
public record SimulateCreditCommand : IZeebeJob, IRequest
{
    public IJob Job { get; set; }
}
