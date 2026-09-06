using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScryTrader.Models;

public class Blueprint
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("game_id")]
    public int GameId { get; set; }

    [JsonPropertyName("category_id")]
    public int CategoryId { get; set; }

    [JsonPropertyName("expansion_id")]
    public int? ExpansionId { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("scryfall_id")]
    public string? ScryfallId { get; set; }

    [JsonPropertyName("card_market_ids")]
    public List<int>? CardMarketIds { get; set; }

    [JsonPropertyName("tcg_player_id")]
    public JsonElement? TcgPlayerId { get; set; }

    [JsonPropertyName("editable_properties")]
    public List<Property>? EditableProperties { get; set; }
}
