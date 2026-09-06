using System.Text.Json.Serialization;

namespace ScryTrader.Models;

public class MarketplaceUserInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = "";

    [JsonPropertyName("can_sell_via_hub")]
    public bool CanSellViaHub { get; set; }

    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; } = "";

    [JsonPropertyName("user_type")]
    public string UserType { get; set; } = "";

    [JsonPropertyName("max_sellable_in24h_quantity")]
    public int? MaxSellableIn24HQuantity { get; set; }
}
