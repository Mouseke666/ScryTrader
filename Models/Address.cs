using System.Text.Json.Serialization;

namespace ScryTrader.Models;

public class Address
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("street")]
    public string Street { get; set; } = "";

    [JsonPropertyName("zip")]
    public string Zip { get; set; } = "";

    [JsonPropertyName("city")]
    public string City { get; set; } = "";

    [JsonPropertyName("state_or_province")]
    public string StateOrProvince { get; set; } = "";

    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; } = "";
}
