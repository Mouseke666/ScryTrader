using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScryTrader.Models;

public class ScryfallCard
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("object")]
    public string Object { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("type_line")]
    public string TypeLine { get; set; } = "";

    [JsonPropertyName("mana_cost")]
    public string ManaCost { get; set; } = "";

    [JsonPropertyName("cmc")]
    public double Cmc { get; set; }

    [JsonPropertyName("colors")]
    public List<string> Colors { get; set; } = [];

    [JsonPropertyName("color_identity")]
    public List<string> ColorIdentity { get; set; } = [];

    [JsonPropertyName("loyalty")]
    public string? Loyalty { get; set; }

    [JsonPropertyName("power")]
    public string? Power { get; set; }

    [JsonPropertyName("toughness")]
    public string? Toughness { get; set; }

    [JsonPropertyName("layout")]
    public string Layout { get; set; } = "";

    [JsonPropertyName("set")]
    public string Set { get; set; } = "";

    [JsonPropertyName("set_name")]
    public string SetName { get; set; } = "";

    [JsonPropertyName("rarity")]
    public string Rarity { get; set; } = "";

    [JsonPropertyName("artist")]
    public string Artist { get; set; } = "";

    [JsonPropertyName("border_mode")]
    public string BorderMode { get; set; } = "";

    [JsonPropertyName("foil")]
    public bool? Foil { get; set; }

    [JsonPropertyName("nonfoil")]
    public bool? NonFoil { get; set; }

    [JsonPropertyName("finish")]
    public List<string>? Finish { get; set; }

    [JsonPropertyName("reserved")]
    public bool? Reserved { get; set; }

    [JsonPropertyName("mint_status")]
    public JsonElement? MintStatus { get; set; }

    [JsonPropertyName("reprint")]
    public bool? Reprint { get; set; }

    [JsonPropertyName("digital")]
    public bool? Digital { get; set; }

    [JsonPropertyName("prices")]
    public Dictionary<string, double?>? Prices { get; set; }

    [JsonPropertyName("unity_id")]
    public string? UnityId { get; set; }

    [JsonPropertyName("tcgplayer_url")]
    public string? TcgPlayerUrl { get; set; }

    [JsonPropertyName("tcgplayer_price")]
    public double? TcgPlayerPrice { get; set; }

    [JsonPropertyName("cardmarket_url")]
    public string? CardMarketUrl { get; set; }

    [JsonPropertyName("cardmarket_price")]
    public double? CardMarketPrice { get; set; }

    [JsonPropertyName("mtgo_id")]
    public int? MtgoId { get; set; }

    [JsonPropertyName("arena_id")]
    public int? ArenaId { get; set; }

    [JsonPropertyName("scryfall_id")]
    public string ScryfallId { get; set; } = "";
}
