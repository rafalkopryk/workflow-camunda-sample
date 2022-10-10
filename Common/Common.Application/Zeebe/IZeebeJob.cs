using Zeebe.Client.Api.Responses;

namespace Common.Application.Zeebe;

public interface IZeebeJob
{
    IJob Job { get; set; }
}