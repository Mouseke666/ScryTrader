using System.Text.Json.Serialization;

namespace ScryTrader.Models;

public class MarketplaceProduct
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("blueprint_id")]
    public int BlueprintId { get; set; }

    [JsonPropertyName("name_en")]
    public string NameEn { get; set; } = "";

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("price")]
    public Price? Price { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("properties_hash")]
    public Dictionary<string, object>? PropertiesHash { get; set; }

    [JsonPropertyName("expansion")]
    public MarketplaceExpansionInfo? Expansion { get; set; }

    [JsonPropertyName("user")]
    public MarketplaceUserInfo User { get; set; } = null!;

    [JsonPropertyName("graded")]
    public bool Graded { get; set; }

    [JsonPropertyName("on_vacation")]
    public bool OnVacation { get; set; }

    [JsonPropertyName("bundle_size")]
    public int BundleSize { get; set; }
}
