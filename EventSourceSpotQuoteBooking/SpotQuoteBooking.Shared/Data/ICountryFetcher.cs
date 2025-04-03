namespace SpotQuoteBooking.Shared.Data;

public interface ICountryFetcher
{
    IReadOnlyCollection<Country> GetCountries();
}
