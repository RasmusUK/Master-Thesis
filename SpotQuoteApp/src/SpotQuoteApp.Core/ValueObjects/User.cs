using EventSourcingFramework.Core.Attributes;

namespace SpotQuoteApp.Core.ValueObjects;

public record User(Guid Id, string Name, string Email, string Phone, string Office)
{
    public Guid Id { get; } = Id;

    [PersonalData]
    public string Name { get; } = Name;

    [PersonalData]
    public string Email { get; } = Email;

    [PersonalData]
    public string Phone { get; } = Phone;

    [PersonalData]
    public string Office { get; } = Office;

    public override string ToString()
    {
        return $"{Name} ({Email})";
    }
}
