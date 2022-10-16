namespace Applications.Application.UseCases.GetApplication.Dto;

public record GetApplicationQueryCustomerPersonalDataDto
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Pesel { get; init; }
}
