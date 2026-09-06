using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScryTrader.Models;

public class Property
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("possible_values")]
    public JsonElement[]? PossibleValues { get; set; }
}
