using ScryTrader.Models;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using ScryTrader.Configuration;
using System.Text.Json;

namespace ScryTrader.Services;

public class CardTraderClient
{
    private readonly HttpClient _httpClient;

    public CardTraderClient(HttpClient httpClient, CardTraderOptions options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.AuthToken);
    }

    public async Task<List<Game>> GetGamesAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<Game>>>("games");
        return response?.Array ?? [];
    }

    public async Task<List<Expansion>> GetExpansionsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Expansion>>("expansions") ?? [];
    }

    public async Task<List<Blueprint>> GetBlueprintsAsync(int expansionId)
    {
        return await _httpClient.GetFromJsonAsync<List<Blueprint>>($"blueprints/export?expansion_id={expansionId}") ?? [];        
    }

    public async Task<List<MarketplaceProduct>> GetMarketplaceProductsAsync(int blueprintId, bool? foil = null, string? language = null)
    {
        var queryParams = new List<string> { $"blueprint_id={blueprintId}" };
        
        if (foil.HasValue)
            queryParams.Add($"foil={foil.Value.ToString().ToLowerInvariant()}");
        
        if (!string.IsNullOrEmpty(language))
            queryParams.Add($"language={language}");

        var queryString = string.Join("&", queryParams);
        var data = await _httpClient.GetFromJsonAsync<Dictionary<int, List<MarketplaceProduct>>>($"marketplace/products?{queryString}") ?? [];
        
        return data.Values.SelectMany(x => x).ToList();
    }
        
}