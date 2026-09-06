using System.Net.Http;
using System.Text.Json;
using ScryTrader.Models;

namespace ScryTrader.Services;

public class ScryfallClient
{
    private readonly HttpClient _httpClient;

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
        return JsonSerializer.Deserialize<ScryfallCard>(response, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            Converters = { new PricesConverter() }
        });
    }

    public async Task<ScryfallCard?> GetCardByCollectorAsync(string setCode, string collectorNumber)
    {
        var cleaned = collectorNumber.Replace("★", "").Trim();
        var response = await _httpClient.GetStringAsync($"/cards/{setCode}/{cleaned}");
        return JsonSerializer.Deserialize<ScryfallCard>(response, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            Converters = { new PricesConverter() }
        });
    }
}
