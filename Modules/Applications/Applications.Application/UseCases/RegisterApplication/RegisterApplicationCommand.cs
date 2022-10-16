using Applications.Application.UseCases.RegisterApplication.Dto;
using MediatR;

namespace Applications.Application.UseCases.RegisterApplication;

public record RegisterApplicationCommand : IRequest
{
    public string ApplicationId { get; init; } 
    public RegisterApplicationCommandCreditApplicationDto CreditApplication { get; init; }
}
