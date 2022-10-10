using Common.Application.Zeebe;
using MediatR;
using Zeebe.Client.Api.Responses;

namespace Applications.Application.UseCases.NotifyApplicationRegistered;

[ZeebeJob(JobType = "notify-application-registered")]
public record NotifyApplicationRegisteredCommand : IZeebeJob, IRequest
{
    public IJob Job { get; set; }
}
