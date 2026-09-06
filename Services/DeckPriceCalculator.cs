using ScryTrader.Models;

namespace ScryTrader.Services;

public class DeckPriceCalculator
{
    private readonly CardTraderClient _cardTrader;
    private readonly BlueprintMatcher _blueprintMatcher;
    private readonly ScryfallClient _scryfall;

    public DeckPriceCalculator(CardTraderClient cardTrader, BlueprintMatcher blueprintMatcher, ScryfallClient scryfall)
    {
        _cardTrader = cardTrader;
        _blueprintMatcher = blueprintMatcher;
        _scryfall = scryfall;
    }

    public async Task<decimal?> GetCardPrice(DeckCard card, List<Expansion> expansions)
    {
        ScryfallCard? scryFallCard = await _scryfall.GetCardByCollectorAsync(
            card.Printing.SetCode,
            card.Printing.CollectorNumber);

        if (scryFallCard == null)
        {
            return null;
        }

        List<Blueprint> bluePrintsFound = await _blueprintMatcher.FindBlueprints(card, scryFallCard, expansions);

        if (bluePrintsFound.Count == 1)
        {
            return await _cardTrader.GetCheapestPrice(
                bluePrintsFound.First().Id,
                CardCondition.NearMint);
        }

        return null;
    }
}