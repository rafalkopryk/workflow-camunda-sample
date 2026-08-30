namespace Applications.Application.Features.RegisterApplication.Dto;

public record RegisterApplicationCommandCustomerPersonalDto
{
    public string FirstName  { get; init; }
    public string LastName { get; init; }
    public string DocumentId { get; init; }
}
