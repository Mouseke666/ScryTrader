using System.Text.Json.Serialization;

namespace ScryTrader.Models;

public class MarketplaceExpansionInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; } = "";

    [JsonPropertyName("name_en")]
    public string NameEn { get; set; } = "";
}
