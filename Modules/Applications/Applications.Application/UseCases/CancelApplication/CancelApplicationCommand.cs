using CSharpFunctionalExtensions;
using MediatR;

namespace Applications.Application.UseCases.CancelApplication;

public record CancelApplicationCommand(string ApplicationId) : IRequest<Result>;
