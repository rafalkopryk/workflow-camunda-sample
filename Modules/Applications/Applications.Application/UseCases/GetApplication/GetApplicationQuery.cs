using MediatR;

namespace Applications.Application.UseCases.GetApplication;

public record GetApplicationQuery(Guid ApplicationId) : IRequest<GetApplicationQueryResponse>;
