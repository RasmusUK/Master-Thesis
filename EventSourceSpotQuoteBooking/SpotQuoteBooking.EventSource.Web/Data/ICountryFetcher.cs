namespace SpotQuoteBooking.EventSource.Web.Data;

public interface ICountryFetcher
{
    IReadOnlyCollection<Country> GetCountries();
}
