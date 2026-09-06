using System.Text;
using ScryTrader.Services;
using ScryTrader.Configuration;
using Microsoft.Extensions.Configuration;
using ScryTrader.Models;

namespace ScryTrader;

public class ScryTraderApplication
{
    private CardTraderClient _cardTrader;
    private ScryfallClient _scryfall;

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

            Game? game = await GetGameByName("Magic");
            List<Expansion> expansions = await GetExpansionsByGame(game);

            var parser = new MoxfieldDeckParser();

            Deck deck = parser.Parse("cards.txt");

            decimal totalPrice = 0;

            foreach (DeckCard card in deck.Cards)
            {
                ScryfallCard? scryFallCard = await GetScryfallCardAsync(card.Printing.SetCode, card.Printing.CollectorNumber);
                var matchingExpansions = expansions.Where(x => x.Code.Contains(card.Printing.SetCode, StringComparison.CurrentCultureIgnoreCase)).ToList();
                List<Blueprint> bluePrintsFound = new List<Blueprint>();
                foreach (var expansion in matchingExpansions)
                {
                    var bluePrints = await GetBluePrints(expansion);                    
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
                bluePrintsFound = bluePrintsFound.DistinctBy(x => x.Id).ToList();

                if (bluePrintsFound.Count == 1)
                {
                    var products = await GetMarketplaceProduct(bluePrintsFound.First().Id);
                    products = products.Where(x => x.User.CanSellViaHub).ToList();
                    products = products.Where(x => x.PropertiesHash != null &&  x.PropertiesHash.TryGetValue("condition", out var condition) && condition?.ToString() == "Near Mint").ToList();
                    var cheapestProduct = products.OrderBy(x => x.Price?.Cents).FirstOrDefault();
                    if (cheapestProduct != null && cheapestProduct.Price != null)
                    {
                        int cents = cheapestProduct.Price.Cents;
                        decimal price = cents / 100m;
                        Console.WriteLine($"{card.Printing.Name} - €{price:F2}");
                        totalPrice += price;
                    }
                }
                else if(bluePrintsFound.Count > 1)
                {
                    var exactExpansion = matchingExpansions.FirstOrDefault(x => 
                        x.Code.Equals(card.Printing.SetCode, StringComparison.CurrentCultureIgnoreCase));
                    
                    if (exactExpansion != null)
                    {
                        var exactBlueprints = await GetBluePrints(exactExpansion);
                        var filteredExact = exactBlueprints.Where(x => x.Name == card.Printing.Name && !string.IsNullOrEmpty(x.ScryfallId) && x.ScryfallId == scryFallCard.Id).ToList();
                        
                        if (filteredExact.Count > 0)
                        {
                            bluePrintsFound = filteredExact.DistinctBy(x => x.Id).ToList();
                        }
                        else
                        {
                            bluePrintsFound = exactBlueprints.Where(x => x.Name == card.Printing.Name && string.IsNullOrEmpty(x.ScryfallId)).DistinctBy(x => x.Id).ToList();
                        }

                        if (bluePrintsFound.Count == 1)
                        {
                            var products = await GetMarketplaceProduct(bluePrintsFound.First().Id);
                            products = products.Where(x => x.User.CanSellViaHub).ToList();
                            products = products.Where(x => x.PropertiesHash != null && x.PropertiesHash.TryGetValue("condition", out var condition) && condition?.ToString() == "Near Mint").ToList();
                            var cheapestProduct = products.OrderBy(x => x.Price?.Cents).FirstOrDefault();
                            if (cheapestProduct != null && cheapestProduct.Price != null)
                            {
                            int cents = cheapestProduct.Price.Cents;
                            decimal price = cents / 100m;
                            Console.WriteLine($"{card.Printing.Name} - €{price:F2}");
                            totalPrice += price;
                        }
                        }
                    }
                }
                else
                {
                    var partialSetCode = card.Printing.SetCode.Substring(0, Math.Min(2, card.Printing.SetCode.Length)).ToLower();
                    var fallbackExpansions = expansions.Where(x => x.Code.ToLower().Contains(partialSetCode)).ToList();
                    
                    if (fallbackExpansions.Any())
                    {
                        foreach (var expansion in fallbackExpansions)
                        {
                            var bluePrints = await GetBluePrints(expansion);
                            var exactMatches = bluePrints.Where(x => !string.IsNullOrEmpty(x.ScryfallId) && x.ScryfallId == scryFallCard.Id).ToList();
                            if (exactMatches.Count > 0)
                                bluePrintsFound.AddRange(exactMatches);
                            else
                                bluePrintsFound.AddRange(bluePrints.Where(x => x.Name == card.Printing.Name && string.IsNullOrEmpty(x.ScryfallId)));
                        }
                        
                        bluePrintsFound = bluePrintsFound.DistinctBy(x => x.Id).ToList();
                        
                        if (bluePrintsFound.Count == 1)
                        {
                            var products = await GetMarketplaceProduct(bluePrintsFound.First().Id);
                            products = products.Where(x => x.User.CanSellViaHub).ToList();
                            products = products.Where(x => x.PropertiesHash != null && x.PropertiesHash.TryGetValue("condition", out var condition) && condition?.ToString() == "Near Mint").ToList();
                            var cheapestProduct = products.OrderBy(x => x.Price?.Cents).FirstOrDefault();
                            if (cheapestProduct != null && cheapestProduct.Price != null)
                            {
                                int cents = cheapestProduct.Price.Cents;
                                decimal price = cents / 100m;
                                Console.WriteLine($"{card.Printing.Name} - €{price:F2}");
                                totalPrice += price;
                            }
                        }
                    }
                }
            }

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

    private async Task<List<Blueprint>> GetBluePrints(Expansion expansion)
    {
        var result = await _cardTrader.GetBlueprintsAsync(expansion.Id);        
        return result;
    }

    private async Task<Game> GetGameByName(string name)
    {
        var games = await _cardTrader.GetGamesAsync();
        return games.FirstOrDefault(x => x.Name == name) ?? throw new InvalidOperationException($"Game '{name}' not found.");
    }

    private async Task<List<Expansion>> GetExpansionsByGame(Game game)
    {        
        var expansions = await _cardTrader.GetExpansionsAsync();
        List<Expansion> mtgExpansions = expansions.Where(x => x.GameId == game.Id).ToList();
        return mtgExpansions;
    }

    private async Task<List<MarketplaceProduct>> GetMarketplaceProduct(int bluePrintId)
    {
        return await _cardTrader.GetMarketplaceProductsAsync(bluePrintId);
    }
  
    private async Task<ScryfallCard?> GetScryfallCardAsync(string setCode, string collectorNumber)
    {
        return await _scryfall!.GetCardByCollectorAsync(setCode, collectorNumber);
    }
}