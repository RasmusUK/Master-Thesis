namespace SpotQuoteApp.Web.Data;

public interface ICountryFetcher
{
    IReadOnlyCollection<Country> GetCountries();
}
