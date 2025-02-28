namespace EventSource.Core.Test;

public record Address(string Street, string City, string State, string ZipCode)
{
    private string Country = "Country";

    public string GetCountry() => Country;
}
