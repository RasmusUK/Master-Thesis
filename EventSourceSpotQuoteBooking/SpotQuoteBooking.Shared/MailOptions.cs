namespace SpotQuoteBooking.Shared;

public class MailOptions
{
    public ICollection<string> MailTo { get; set; } = new List<string>();
    public bool SendCopyToMe { get; set; }
    public bool ShowCostSpec { get; set; }
    public string Comments { get; set; } = string.Empty;
}
