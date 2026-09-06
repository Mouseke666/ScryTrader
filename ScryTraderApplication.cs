using System.Text;
using ScryTrader.Models;
using ScryTrader.Services;
using ScryTrader.Configuration;
using Microsoft.Extensions.Configuration;

namespace ScryTrader;

public class ScryTraderApplication
{
    private CardTraderClient _cardTrader;
    private ScryfallClient _scryfall;
    private readonly Dictionary<int, List<Blueprint>> _blueprintCache = new();
    private readonly Dictionary<int, List<MarketplaceProduct>> _marketplaceProductCache = new();
    private readonly Dictionary<string, ScryfallCard?> _scryfallCache = new();

    public ScryTraderApplication()
    {
        _cardTrader = null!;
        _scryfall = null!;
    }

    public async Task<int> RunAsync()
    {
        try
        {
            var options = LoadConfiguration();
            using var httpClient = new HttpClient();
            _cardTrader = new CardTraderClient(httpClient, options);

            using var scryfallHttpClient = new HttpClient();
            _scryfall = new ScryfallClient(scryfallHttpClient);

            Console.OutputEncoding = Encoding.UTF8;

            Game? game = await _cardTrader.GetGameByNameAsync("Magic");
            List<Expansion> expansions = await _cardTrader.GetExpansionsByGameAsync(game);

            var parser = new MoxfieldDeckParser();

            Deck deck = parser.Parse("cards.txt");

            decimal totalPrice = await CalculateDeckPrice(deck, expansions);            

            Console.WriteLine($"Total price for deck: €{totalPrice:F2}");
            return 0;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"ScryTrader API error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return 1;
        }
    }
    
    private static CardTraderOptions LoadConfiguration()
    {
        var assemblyDir = AppContext.BaseDirectory;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(assemblyDir)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var options = configuration
            .GetSection("CardTrader")
            .Get<CardTraderOptions>()
            ?? throw new InvalidOperationException(
                "CardTrader configuration is missing.");

        if (string.IsNullOrWhiteSpace(options.AuthToken))
        {
            throw new InvalidOperationException("CardTrader API token is missing.");
        }

        return options;
    }

    //private async Task<List<Blueprint>> GetBluePrints(Expansion expansion)
    //{
    //    if (_blueprintCache.TryGetValue(expansion.Id, out var bluePrints))
    //    {
    //        return bluePrints;
    //    }

    //    var result = await _cardTrader.GetBlueprintsAsync(expansion.Id);
    //    _blueprintCache[expansion.Id] = result;

    //    return result;
    //}
        
    private async Task<List<MarketplaceProduct>> GetMarketplaceProduct(int bluePrintId)
    {
        if (_marketplaceProductCache.TryGetValue(bluePrintId, out var products))
        {
            return products;
        }

        var result = await _cardTrader.GetMarketplaceProductsAsync(bluePrintId);
        _marketplaceProductCache[bluePrintId] = result;

        return result;
    }

    private async Task<ScryfallCard?> GetScryfallCardAsync(string setCode, string collectorNumber)
    {
        var cacheKey = $"{setCode}:{collectorNumber}";

        if (_scryfallCache.TryGetValue(cacheKey, out var scryfallCard))
        {
            return scryfallCard;
        }

        scryfallCard = await _scryfall.GetCardByCollectorAsync(setCode, collectorNumber);
        _scryfallCache[cacheKey] = scryfallCard;

        return scryfallCard;
    }

    private async Task<decimal?> GetCheapestPrice(int blueprintId, CardCondition condition)
    {
        var products = await GetMarketplaceProduct(blueprintId);

        products = products
            .Where(x => x.User.CanSellViaHub)
            .Where(x => x.PropertiesHash != null &&
                        x.PropertiesHash.TryGetValue("condition", out var productCondition) &&
                        productCondition?.ToString() == condition.ToCardTraderValue())
            .Where(x => x.Price != null)
            .ToList();

        var cheapestProduct = products.MinBy(x => x.Price!.Cents);

        return cheapestProduct?.Price?.Cents / 100m;
    }

    private async Task<decimal?> GetCardPrice(DeckCard card, List<Expansion> expansions)
    {
        ScryfallCard? scryFallCard = await GetScryfallCardAsync(card.Printing.SetCode, card.Printing.CollectorNumber);

        if (scryFallCard == null)
        {
            return null;
        }

        List<Blueprint> bluePrintsFound = await FindBlueprints(card, scryFallCard, expansions);

        if (bluePrintsFound.Count == 1)
        {
            return await GetCheapestPrice(bluePrintsFound.First().Id, CardCondition.NearMint);
        }

        return null;
    }

    private async Task<List<Blueprint>> FindBlueprints(DeckCard card, ScryfallCard scryFallCard, List<Expansion> expansions)
    {
        var matchingExpansions = expansions.Where(x => x.Code.Contains(card.Printing.SetCode, StringComparison.CurrentCultureIgnoreCase)).ToList();

        List<Blueprint> bluePrintsFound = await FindBlueprintsInExpansions(card, scryFallCard, matchingExpansions);

        bluePrintsFound = bluePrintsFound.DistinctBy(x => x.Id).ToList();

        if (bluePrintsFound.Count > 1)
        {
            bluePrintsFound = await ResolveMultipleBlueprints(card, scryFallCard, matchingExpansions);
        }
        else if (bluePrintsFound.Count == 0)
        {
            bluePrintsFound = await FindBlueprintsUsingFallback(card, scryFallCard, expansions);
        }

        return bluePrintsFound;
    }

    private async Task<List<Blueprint>> FindBlueprintsInExpansions(DeckCard card, ScryfallCard scryFallCard, List<Expansion> matchingExpansions)
    {
        List<Blueprint> bluePrintsFound = new List<Blueprint>();

        foreach (var expansion in matchingExpansions)
        {
            var bluePrints = await _cardTrader.GetBlueprintsByExpansionAsync(expansion);
            var exactMatches = bluePrints.Where(x => !string.IsNullOrEmpty(x.ScryfallId) && x.ScryfallId == scryFallCard.Id).ToList();

            if (exactMatches.Count > 0)
            {
                bluePrintsFound.AddRange(exactMatches);
            }
            else
            {
                bluePrintsFound.AddRange(bluePrints.Where(x => x.Name == card.Printing.Name && string.IsNullOrEmpty(x.ScryfallId)));
            }
        }

        return bluePrintsFound;
    }

    private async Task<List<Blueprint>> ResolveMultipleBlueprints(DeckCard card, ScryfallCard scryFallCard, List<Expansion> matchingExpansions)
    {
        var exactExpansion = matchingExpansions.FirstOrDefault(x => x.Code.Equals(card.Printing.SetCode, StringComparison.CurrentCultureIgnoreCase));

        if (exactExpansion == null)
        {
            return new List<Blueprint>();
        }

        var exactBlueprints = await _cardTrader.GetBlueprintsByExpansionAsync(exactExpansion);

        var filteredExact = exactBlueprints
            .Where(x => x.Name == card.Printing.Name &&
                        !string.IsNullOrEmpty(x.ScryfallId) &&
                        x.ScryfallId == scryFallCard.Id)
            .ToList();

        if (filteredExact.Count > 0)
        {
            return filteredExact.DistinctBy(x => x.Id).ToList();
        }

        return exactBlueprints
            .Where(x => x.Name == card.Printing.Name && string.IsNullOrEmpty(x.ScryfallId))
            .DistinctBy(x => x.Id)
            .ToList();
    }

    private async Task<List<Blueprint>> FindBlueprintsUsingFallback(DeckCard card, ScryfallCard scryFallCard, List<Expansion> expansions)
    {
        var partialSetCode = card.Printing.SetCode.Substring(0, Math.Min(2, card.Printing.SetCode.Length)).ToLower();
        var fallbackExpansions = expansions.Where(x => x.Code.ToLower().Contains(partialSetCode)).ToList();

        List<Blueprint> bluePrintsFound = new List<Blueprint>();

        if (fallbackExpansions.Any())
        {
            foreach (var expansion in fallbackExpansions)
            {
                var bluePrints = await _cardTrader.GetBlueprintsByExpansionAsync(expansion);
                var exactMatches = bluePrints.Where(x => !string.IsNullOrEmpty(x.ScryfallId) && x.ScryfallId == scryFallCard.Id).ToList();

                if (exactMatches.Count > 0)
                    bluePrintsFound.AddRange(exactMatches);
                else
                    bluePrintsFound.AddRange(bluePrints.Where(x => x.Name == card.Printing.Name && string.IsNullOrEmpty(x.ScryfallId)));
            }

            bluePrintsFound = bluePrintsFound.DistinctBy(x => x.Id).ToList();
        }

        return bluePrintsFound;
    }

    private async Task<decimal> CalculateDeckPrice(Deck deck, List<Expansion> expansions)
    {
        decimal totalPrice = 0;

        foreach (DeckCard card in deck.Cards)
        {
            var price = await GetCardPrice(card, expansions);

            if (price.HasValue)
            {
                Console.WriteLine($"{card.Printing.Name} - €{price.Value:F2}");
                totalPrice += price.Value;
            }
        }

        return totalPrice;
    }

}