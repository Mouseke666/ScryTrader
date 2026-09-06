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

    public async Task<ProductSelection?> GetCardPriceAsync(DeckCard card, List<Expansion> expansions)
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
            var blueprint = bluePrintsFound.First();
            
            var (price, productId) = await _cardTrader.GetCheapestPriceWithProductAsync(
                blueprint.Id, 
                CardCondition.NearMint, 
                card.Quantity,
                card.Printing.Finish);

            if (price.HasValue)
            {
                return new ProductSelection 
                { 
                    ProductId = productId,
                    Quantity = card.Quantity,
                    PricePerUnit = price.Value / card.Quantity,
                    TotalPrice = price.Value,
                    CardName = card.Printing.Name,
                    Finish = card.Printing.Finish
                };
            }
        }

        return null;
    }
}