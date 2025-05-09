namespace SpotQuoteApp.Application.DTOs;

public class CustomerDto
{
    public Guid Id { get; set; }
    public int ConcurrencyVersion { get; set; }
    public string Name { get; set; }
    public ICollection<UserDto> Users { get; set; } = new List<UserDto>();
}
