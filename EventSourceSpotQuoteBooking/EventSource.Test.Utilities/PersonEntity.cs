using EventSource.Core;
using EventSource.Persistence;

namespace EventSource.Test.Utilities;

public class PersonEntity : Entity
{
    [PersonalData]
    public string Name { get; set; }

    [PersonalData]
    public string Email { get; set; }

    public Address Address { get; set; } = new();
}

public class Address
{
    [PersonalData]
    public string Street { get; set; }

    [PersonalData]
    public string City { get; set; }

    public Location Location { get; set; } = new();
}

public class Location
{
    [PersonalData]
    public decimal Latitude { get; set; }

    [PersonalData]
    public decimal Longitude { get; set; }
}
