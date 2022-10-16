using MediatR;

namespace Applications.Application.UseCases.SignContract;

public record SignContractCommand(string ApplicationId) : IRequest;
