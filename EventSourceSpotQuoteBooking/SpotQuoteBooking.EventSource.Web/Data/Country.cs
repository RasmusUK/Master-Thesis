using Newtonsoft.Json;

namespace SpotQuoteBooking.EventSource.Web.Data;

public class Country
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("alpha-2")]
    public string Code { get; set; }

    public override string ToString() => $"{Code} - {Name}";
}
