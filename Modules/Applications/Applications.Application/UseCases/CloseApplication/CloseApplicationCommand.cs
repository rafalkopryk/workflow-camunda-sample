﻿using Common.Application.Zeebe;
using MediatR;
using Zeebe.Client.Api.Responses;

namespace Applications.Application.UseCases.CloseApplication;

[ZeebeJob(JobType = "close-application", MaxJobsActive = 5, PollIntervalInMs = 100)]
public record CloseApplicationCommand : IZeebeJob, IRequest
{
    public IJob Job { get; set; }
}
