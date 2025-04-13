namespace SpotQuoteBooking.EventSource.Application.DTOs;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<UserDto> Users { get; set; } = new List<UserDto>();
}
