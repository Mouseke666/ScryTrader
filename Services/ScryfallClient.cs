using System.Text.Json;
using ScryTrader.Models;

namespace ScryTrader.Services;

public class ScryfallClient
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, ScryfallCard?> _scryfallCache = new();
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new PricesConverter() }
    };

    public ScryfallClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.scryfall.com");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ScryTrader");
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    }

    public async Task<ScryfallCard?> GetCardAsync(string scryfallId)
    {
        var response = await _httpClient.GetStringAsync($"/cards/{scryfallId}");
        return JsonSerializer.Deserialize<ScryfallCard>(response, _jsonSerializerOptions);
    }

    public async Task<ScryfallCard?> GetCardByCollectorAsync(string setCode, string collectorNumber)
    {
        var cacheKey = $"{setCode}:{collectorNumber}";

        if (_scryfallCache.TryGetValue(cacheKey, out var scryfallCard))
        {
            return scryfallCard;
        }

        var cleaned = collectorNumber.Replace("★", "").Trim();
        var response = await _httpClient.GetStringAsync($"/cards/{setCode}/{cleaned}");

        scryfallCard = JsonSerializer.Deserialize<ScryfallCard>(response, _jsonSerializerOptions);

        _scryfallCache[cacheKey] = scryfallCard;

        return scryfallCard;
    }
}