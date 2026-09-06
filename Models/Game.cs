using System.Text.Json.Serialization;

namespace ScryTrader.Models;

public class Game
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}
