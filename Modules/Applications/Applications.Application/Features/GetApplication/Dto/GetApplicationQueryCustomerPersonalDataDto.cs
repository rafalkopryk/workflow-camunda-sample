namespace Applications.Application.Features.GetApplication.Dto;

public record GetApplicationQueryCustomerPersonalDataDto
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string DocumentId { get; init; }
}
