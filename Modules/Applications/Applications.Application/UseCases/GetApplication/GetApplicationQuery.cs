using MediatR;

namespace Applications.Application.UseCases.GetApplication;

public record GetApplicationQuery(string ApplicationId) : IRequest<GetApplicationQueryResponse>;
