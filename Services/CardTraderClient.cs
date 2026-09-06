using ScryTrader.Models;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using ScryTrader.Configuration;

namespace ScryTrader.Services;

public class CardTraderClient
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<int, List<Blueprint>> _blueprintCache = new();
    private readonly Dictionary<int, List<MarketplaceProduct>> _marketplaceProductCache = new();

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

    public async Task<Game> GetGameByNameAsync(string name)
    {
        var games = await GetGamesAsync();

        return games.FirstOrDefault(x => x.Name == name) ?? throw new InvalidOperationException($"Game '{name}' not found.");
    }

    public async Task<List<Expansion>> GetExpansionsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Expansion>>("expansions") ?? [];
    }

    public async Task<List<Expansion>> GetExpansionsByGameAsync(Game game)
    {
        var expansions = await GetExpansionsAsync();

        return expansions.Where(x => x.GameId == game.Id).ToList();
    }

    public async Task<List<Blueprint>> GetBlueprintsAsync(int expansionId)
    {
        if (_blueprintCache.TryGetValue(expansionId, out var bluePrints))
        {
            return bluePrints;
        }

        var result = await _httpClient.GetFromJsonAsync<List<Blueprint>>(
            $"blueprints/export?expansion_id={expansionId}") ?? [];

        _blueprintCache[expansionId] = result;

        return result;
    }

    public async Task<List<Blueprint>> GetBlueprintsByExpansionAsync(Expansion expansion)
    {
        return await GetBlueprintsAsync(expansion.Id);
    }

    public async Task<List<MarketplaceProduct>> GetMarketplaceProductsAsync(int blueprintId, bool? foil = null, string? language = null)
    {
        if (_marketplaceProductCache.TryGetValue(blueprintId, out var products))
        {
            return products;
        }

        var queryParams = new List<string> { $"blueprint_id={blueprintId}" };

        if (foil.HasValue)
            queryParams.Add($"foil={foil.Value.ToString().ToLowerInvariant()}");

        if (!string.IsNullOrEmpty(language))
            queryParams.Add($"language={language}");

        var queryString = string.Join("&", queryParams);
        var data = await _httpClient.GetFromJsonAsync<Dictionary<int, List<MarketplaceProduct>>>($"marketplace/products?{queryString}") ?? [];

        products = data.Values.SelectMany(x => x).ToList();
        _marketplaceProductCache[blueprintId] = products;

        return products;
    }

    [Obsolete("Use GetCheapestPriceForQuantity(int, CardCondition, int) instead")]
    public async Task<decimal?> GetCheapestPrice(int blueprintId, CardCondition condition)
    {
        return await GetCheapestPriceForQuantity(blueprintId, condition, quantityNeeded: 1);
    }

    public async Task<decimal?> GetCheapestPriceForQuantity(
        int blueprintId, 
        CardCondition condition, 
        int quantityNeeded)
    {
        var products = await GetMarketplaceProductsAsync(blueprintId);

        // Filter: only sellers who can sell via hub AND not on vacation
        var eligibleProducts = products
            .Where(x => x.User.CanSellViaHub)
            .Where(x => !x.OnVacation)
            .Where(x => x.PropertiesHash != null &&
                        x.PropertiesHash.TryGetValue("condition", out var productCondition) &&
                        productCondition?.ToString() == condition.ToCardTraderValue())
            .Where(x => x.Price != null)
            .OrderBy(x => x.Price!.Cents)
            .ToList();

        int remaining = quantityNeeded;
        decimal totalCost = 0;

        foreach (var product in eligibleProducts)
        {
            if (remaining <= 0) break;

            int buyCount = Math.Min(remaining, product.Quantity);
            totalCost += buyCount * product.Price!.Cents / 100m;
            remaining -= buyCount;
        }

        return totalCost > 0 ? totalCost : null;
    }

}