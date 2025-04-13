namespace SpotQuoteBooking.EventSource.Application.DTOs;

public class MailOptionsDto
{
    public bool SendCopyToMe { get; set; }
    public bool ShowCostSpec { get; set; }
    public string Comments { get; set; }
    public ICollection<UserDto> UserRecipients { get; set; } = new List<UserDto>();
}
