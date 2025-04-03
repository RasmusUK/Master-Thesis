using Newtonsoft.Json;

namespace SpotQuoteBooking.Shared.Data;

public class CountryFetcher : ICountryFetcher
{
    public IReadOnlyCollection<Country> GetCountries()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data/Countries.json");
        var json = File.ReadAllText(path);
        var countries = JsonConvert.DeserializeObject<List<Country>>(json);
        if (countries is null)
            throw new Exception("Failed to deserialize Countries.json");
        return countries;
    }
}
