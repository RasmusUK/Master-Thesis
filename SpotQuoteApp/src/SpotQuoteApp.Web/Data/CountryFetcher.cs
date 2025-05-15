using Newtonsoft.Json;

namespace SpotQuoteApp.Web.Data;

public class CountryFetcher : ICountryFetcher
{
    private IReadOnlyCollection<Country> countries;

    public IReadOnlyCollection<Country> GetCountries()
    {
        if (countries is not null && countries.Count > 0)
            return countries.ToList();
        var path = Path.Combine(AppContext.BaseDirectory, "Data/Countries.json");
        var json = File.ReadAllText(path);
        countries = JsonConvert.DeserializeObject<List<Country>>(json);
        if (countries is null)
            throw new Exception("Failed to deserialize Countries.json");
        return countries;
    }
}
