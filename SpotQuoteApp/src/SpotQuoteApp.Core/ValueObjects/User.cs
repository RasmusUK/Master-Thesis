using EventSourcingFramework.Core.Attributes;

namespace SpotQuoteApp.Core.ValueObjects;

public record User(Guid Id, string Name, string Email, string Phone, string Office)
{
    public Guid Id { get; set; } = Id;

    [PersonalData]
    public string Name { get; set; } = Name;

    [PersonalData]
    public string Email { get; set; } = Email;

    [PersonalData]
    public string Phone { get; set; } = Phone;

    public string Office { get; set; } = Office;

    public override string ToString()
    {
        return $"{Name} ({Email})";
    }
}
