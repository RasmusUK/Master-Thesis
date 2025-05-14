using EventSourcingFramework.Core.Models.Entity;
using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Core.AggregateRoots;

public class Location : Entity
{
    public Location(string code, string name, Guid countryId, LocationType type)
    {
        Code = code;
        Name = name;
        CountryId = countryId;
        Type = type;
    }

    public string Code { get; set; }
    public string Name { get; set; }
    public Guid CountryId { get; set; }
    public LocationType Type { get; set; }
}
