using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScryTrader.Services;

public class PricesConverter : JsonConverter<Dictionary<string, double?>>
{
    public override Dictionary<string, double?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        var dict = new Dictionary<string, double?>();
        
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            string key = reader.GetString() ?? "";
            
            double? value = reader.TokenType switch
            {
                JsonTokenType.Number => reader.GetDouble(),
                JsonTokenType.Null => null,
                _ => null // strings en andere types worden null
            };

            dict[key] = value;
        }

        return dict;
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, double?>? value, JsonSerializerOptions options)
    {
        if (value == null)
            return;

        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WriteNumber(kvp.Key, kvp.Value ?? 0);
        }
        writer.WriteEndObject();
    }
}
