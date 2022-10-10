using Applications.Application.UseCases.RegisterApplication.Dto;
using MediatR;

namespace Applications.Application.UseCases.GetProcessAvailability;

public record GetProcessAvailabilityQuery : IRequest<GetProcessAvailabilityQueryResponse>
{
}
