namespace EventSource.Core.Test;

public record Address(string Street, string City, string State, string ZipCode)
{
    public string Country = "Country";

    public string GetCountry() => Country;
}
