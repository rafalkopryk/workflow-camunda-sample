namespace Applications.Application.UseCases.SignContract;

public record SignContractCommand(string ApplicationId);

public record SignContractCommandResponse
{
    public record OK() : SignContractCommandResponse
    {
        public static readonly OK Result = new();
    }

    public record ResourceNotFound() : SignContractCommandResponse
    {
        public static readonly ResourceNotFound Result = new();
    }
}