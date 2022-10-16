namespace Applications.Application.UseCases.RegisterApplication.Dto;

public record RegisterApplicationCommandCustomerPersonalDto
{
    public string FirstName  { get; init; }
    public string LastName { get; init; }
    public string Pesel { get; init; }
}
