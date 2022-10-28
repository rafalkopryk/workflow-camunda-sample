using Applications.Application.UseCases.RegisterApplication.Dto;
using CSharpFunctionalExtensions;
using MediatR;

namespace Applications.Application.UseCases.RegisterApplication;

public record RegisterApplicationCommand : IRequest<Result>
{
    public string ApplicationId { get; init; } 
    public RegisterApplicationCommandCreditApplicationDto CreditApplication { get; init; }
}
