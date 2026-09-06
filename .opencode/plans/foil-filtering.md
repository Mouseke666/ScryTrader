# Foil Filtering Implementation Plan

## Problem Statement

The Moxfield deck parser correctly parses *F* (foil) markers from card lines like:
`
1 Bloodthirster (40K) 73★ *F*
`

However, the foil information is **not used** when searching for marketplace products. The current flow:

`
MoxfieldParser → DeckCard.Printing.Finish = CardFinish.Foil ✅
    ↓
BlueprintMatcher.FindBlueprints() - ignores finish ❌
    ↓
GetCheapestPriceForQuantity(blueprintId, condition, quantity) - no foil filter ❌
`

---

## Solution Overview

Pass CardFinish through the entire pipeline and use it to filter marketplace products.

### Data Flow (After)

`
MoxfieldParser → DeckCard.Printing.Finish = CardFinish.Foil ✅
    ↓
DeckPriceCalculator.GetCardPrice() - passes finish ✅
    ↓
BlueprintMatcher.FindBlueprints() - ignores finish (blueprints don't track finish) ✅
    ↓
GetCheapestPriceForQuantity(blueprintId, condition, quantity, finish) ✅
    ↓
GetMarketplaceProductsAsync(blueprintId, foil: true/false) ✅
`

---

## Implementation Steps

### Step 1: Update CardTraderClient.GetCheapestPriceForQuantity()

**File:** Services/CardTraderClient.cs

**Changes:**
- Add CardFinish finish = CardFinish.NonFoil parameter (default for backwards compatibility)
- Pass oil to GetMarketplaceProductsAsync() based on finish type
- Etched cards are always foil, so they use oil: true

`csharp
public async Task<decimal?> GetCheapestPriceForQuantity(
    int blueprintId, 
    CardCondition condition, 
    int quantityNeeded,
    CardFinish finish = CardFinish.NonFoil)
{
    bool? foilParam = finish switch
    {
        CardFinish.Foil => true,
        CardFinish.Etched => true,  // Etched is always foil
        _ => false  // NonFoil
    };

    var products = await GetMarketplaceProductsAsync(blueprintId, foil: foilParam);

    // Rest of method unchanged (condition filter, etc.)
}
`

### Step 2: Update DeckPriceCalculator.GetCardPrice()

**File:** Services/DeckPriceCalculator.cs

**Changes:**
- Pass card.Printing.Finish to GetCheapestPriceForQuantity()

`csharp
return await _cardTrader.GetCheapestPriceForQuantity(
    bluePrintsFound.First().Id, 
    CardCondition.NearMint, 
    card.Quantity,
    card.Printing.Finish);  // ← Added finish parameter
`

---

## Trade-offs

| Aspect | Detail |
|--------|--------|
| **Accuracy** | Foil cards will now return foil-specific prices (typically higher) |
| **Backwards compat** | Default parameter CardFinish.NonFoil ensures old code still works |
| **API calls** | Uses existing oil query param in GetMarketplaceProductsAsync() - no new API calls |
| **Etched handling** | Treated as foil (correct behavior since etched cards are always foil) |

---

## Testing Strategy

1. Test with non-foil card: should return non-foil price
2. Test with foil card (*F*): should return foil price
3. Test with etched card (*E*): should return foil price (etched is always foil)
4. Test fallback when no matching finish available

---

## Files Modified

| File | Change |
|------|--------|
| Services/CardTraderClient.cs | Add inish parameter, pass to GetMarketplaceProductsAsync() |
| Services/DeckPriceCalculator.cs | Pass card.Printing.Finish to GetCheapestPriceForQuantity() |
