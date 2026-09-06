using System.Text.Json.Serialization;

namespace ScryTrader.Models;

public class Price
{
    [JsonPropertyName("cents")]
    public int Cents { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "";
}
