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

            decimal totalPrice = await CalculateDeckPriceAsync(deck, expansions);

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

    private async Task<decimal> CalculateDeckPriceAsync(Deck deck, List<Expansion> expansions)
    {
        decimal totalPrice = 0;

        foreach (DeckCard card in deck.Cards)
        {
            var price = await _deckPriceCalculator.GetCardPrice(card, expansions);

            if (price.HasValue)
            {
                Console.WriteLine($"{card.Quantity}x {card.Printing.Name} - €{price.Value:F2}");
                totalPrice += price.Value;
            }
        }

        return totalPrice;
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