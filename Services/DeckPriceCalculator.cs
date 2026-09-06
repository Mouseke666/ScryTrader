using ScryTrader.Models;

namespace ScryTrader.Services;

public class DeckPriceCalculator
{
    private readonly CardTraderClient _cardTrader;
    private readonly ScryfallClient _scryfall;

    public DeckPriceCalculator(CardTraderClient cardTrader, ScryfallClient scryfall)
    {
        _cardTrader = cardTrader;
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

        List<Blueprint> bluePrintsFound = await FindBlueprints(card, scryFallCard, expansions);

        if (bluePrintsFound.Count == 1)
        {
            return await _cardTrader.GetCheapestPrice(
                bluePrintsFound.First().Id,
                CardCondition.NearMint);
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
}