using Applications.Application.UseCases.RegisterApplication.Dto;
using MediatR;

namespace Applications.Application.UseCases.RegisterApplication;

public record RegisterApplicationCommand : IRequest<RegisterApplicationCommandResponse>
{
    public string ApplicationId { get; init; } 
    public RegisterApplicationCommandCreditApplicationDto CreditApplication { get; init; }
    public string ProcessCode { get; init; }
}

public abstract record RegisterApplicationCommandResponse
{
    public record OK() : RegisterApplicationCommandResponse
    {
        public static readonly OK Result = new (); 
    }

    public record ResourceExists() : RegisterApplicationCommandResponse
    {
        public static readonly ResourceExists Result = new();
    }
}