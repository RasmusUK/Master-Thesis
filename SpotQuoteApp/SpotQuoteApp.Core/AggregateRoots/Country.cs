using EventSourcingFramework.Persistence;

namespace SpotQuoteApp.Core.AggregateRoots;

public class Country : Entity
{
    public Country(string name, string code)
    {
        Name = name;
        Code = code;
    }

    public string Name { get; set; }
    public string Code { get; set; }
}
