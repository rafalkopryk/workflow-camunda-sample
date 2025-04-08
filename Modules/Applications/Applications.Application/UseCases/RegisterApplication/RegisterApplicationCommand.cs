using Applications.Application.UseCases.RegisterApplication.Dto;

namespace Applications.Application.UseCases.RegisterApplication;

public record RegisterApplicationCommand
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