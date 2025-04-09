using EventSource.Core;

namespace SpotQuoteBooking.Shared;

public class User
{
    [PersonalData]
    public string Name { get; set; }

    [PersonalData]
    public string Email { get; set; }

    [PersonalData]
    public string Phone { get; set; }

    [PersonalData]
    public string Office { get; set; }

    public override string ToString()
    {
        return $"{Name} ({Email})";
    }
}
