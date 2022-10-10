using Applications.Application.UseCases.RegisterApplication.Dto;
using MediatR;

namespace Applications.Application.UseCases.RegisterApplication;

public record RegisterApplicationCommand : IRequest
{
    public Guid ApplicationId { get; init; } 
    public ApplicationDto Application { get; init; }
}
