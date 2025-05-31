namespace Credit.Front.Blazor.Client.Components.CustomerPersonalData;

public interface IWithCustomerPersonal
{
    CustomerPersonalDto CustomerPersonalData { get; init; }
}

public record CustomerPersonalDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DocumentId { get; set; }
}
