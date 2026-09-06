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
    private BlueprintMatcher _blueprintMatcher;
    private DeckPriceCalculator _deckPriceCalculator;

    public ScryTraderApplication()
    {
        _cardTrader = null!;
        _scryfall = null!;
        _blueprintMatcher = null!;
        _deckPriceCalculator = null!;
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

            _blueprintMatcher = new BlueprintMatcher(_cardTrader);
            _deckPriceCalculator = new DeckPriceCalculator(_cardTrader, _blueprintMatcher, _scryfall);

            Console.OutputEncoding = Encoding.UTF8;

            Game? game = await _cardTrader.GetGameByNameAsync("Magic");
            List<Expansion> expansions = await _cardTrader.GetExpansionsByGameAsync(game);

            var parser = new MoxfieldDeckParser();

            Deck deck = parser.Parse("cards.txt");

            var selections = await CalculateDeckSelectionsAsync(deck, expansions);

            if (selections.Count == 0)
            {
                Console.WriteLine("No cards found with prices.");
                return 1;
            }

            // Merge quantities for same product
            var mergedSelections = selections
                .GroupBy(s => (s.ProductId, s.Finish))
                .Select(g => new ProductSelection
                {
                    ProductId = g.Key.ProductId,
                    Quantity = g.Sum(x => x.Quantity),
                    PricePerUnit = g.First().PricePerUnit,
                    TotalPrice = g.Sum(x => x.TotalPrice),
                    CardName = g.First().CardName,
                    Finish = g.First().Finish
                })
                .ToList();

            // Show cart summary
            var cartSummary = new CartSummary 
            { 
                Items = mergedSelections, 
                TotalPrice = selections.Sum(s => s.TotalPrice) 
            };
            cartSummary.Print();

            // Ask user confirmation
            Console.Write("\nAdd to cart? (y/n): ");
            var answer = Console.ReadLine()?.ToLower();

            if (answer == "y")
            {
                // Add each product to cart
                int successCount = 0;
                int failCount = 0;

                foreach (var selection in mergedSelections)
                {
                    var result = await _cardTrader.AddToCartAsync(
                        selection.ProductId, 
                        selection.Quantity, 
                        options.BillingAddress!, 
                        options.ShippingAddress!, 
                        viaCardTraderZero: true);

                    if (result.Success)
                    {
                        Console.WriteLine($"✓ Added: {result.Quantity}x {selection.CardName}");
                        successCount++;
                    }
                    else
                    {
                        Console.WriteLine($"✗ Failed: {selection.CardName} ({selection.Quantity}x) - {result.ErrorMessage}");
                        failCount++;
                    }
                }

                Console.WriteLine($"\nCart summary: {successCount} items added, {failCount} failed");
            }

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

    private async Task<List<ProductSelection>> CalculateDeckSelectionsAsync(Deck deck, List<Expansion> expansions)
    {
        var selections = new List<ProductSelection>();

        foreach (DeckCard card in deck.Cards)
        {
            var selection = await _deckPriceCalculator.GetCardPriceAsync(card, expansions);

            if (selection != null)
            {
                Console.WriteLine($"{selection.Quantity}x {selection.CardName} - €{selection.TotalPrice:F2}");
                selections.Add(selection);
            }
        }

        return selections;
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
}