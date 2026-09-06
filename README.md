# ScryTrader

.NET console application that automates purchasing Magic: The Gathering cards from the Card Trader marketplace. It parses your decklist, finds the cheapest available products across multiple sellers, and adds them to your Cart Trader shopping cart with a single confirmation.

## Features

- **Decklist Parsing**: Reads Moxfield-style decklists (`cards.txt`) with quantity, set code, collector number, and foil/etched markers
- **Dual API Integration**: Combines Scryfall.com (card database) with CardTrader.com (marketplace) APIs
- **Smart Blueprint Matching**: Maps physical card printings to Card Trader's internal "blueprint" system using Scryfall IDs as the primary matching key
- **Price Calculation**: Finds the cheapest available products for each card in Near Mint condition, aggregating across multiple sellers if needed to fulfill quantity requirements
- **Foil/Non-Foil/Etched Support**: Correctly filters marketplace products by finish type (non-foil, foil, etched)
- **Cart Automation**: Optionally adds all calculated selections to the Card Trader shopping cart with auto-filled shipping/billing addresses

## Installation

1. Clone the repository:
   ```bash
   git clone <your-repo-url>
   cd ScryTrader
   ```

2. Install .NET 10 SDK or ensure it's available in your environment

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

## Configuration

### Get API Token from Card Trader Profile

You need a valid authentication token from Card Trader to access their API. To obtain this token:

1. Log in to [Card Trader](https://www.cardtrader.com)
2. Go to **Settings** → **API** section (or your profile settings page)
3. Copy the auth token provided there

### Setup `appsettings.json`

Create an `appsettings.json` file in the project root with the following structure:

```json
{
  "CardTrader": {
    "AuthToken": "your_api_token_here",
    "ShippingAddress": {
      "Name": "Your Name",
      "Street": "Street 123",
      "Zip": "50143",
      "City": "Tilburg",
      "StateOrProvince": "NB",
      "CountryCode": "NL"
    },
    "BillingAddress": {
      "Name": "Your Name",
      "Street": "Street 123",
      "Zip": "50143",
      "City": "Tilburg",
      "StateOrProvince": "NB",
      "CountryCode": "NL"
    }
  }
}
```

**Important**: Both `ShippingAddress` and `BillingAddress` are required for cart automation. Update the address fields with your actual shipping and billing information.

## Usage

1. Add your decklist to `cards.txt` in Moxfield format (see Decklist Format below)

2. Run the application:
   ```bash
   dotnet run
   ```

3. The application will:
   - Load configuration from `appsettings.json`
   - Fetch game and expansion data from Card Trader API
   - Parse your decklist from `cards.txt`
   - Look up each card via Scryfall API
   - Find matching blueprints in Card Trader
   - Calculate the cheapest price for each card (Near Mint condition, English language)
   - Display a cart summary with all items and total price

4. Review the output:
   ```
   1x Gogo, Mysterious Mime - €2.50
   9x Mountain - €4.50
   
   Cart summary: 2 items added, 0 failed
   Total price: €7.00
   ```

5. When prompted `Add to cart? (y/n):`, type `y` to add all items to your Card Trader cart or `n` to skip

### Decklist Format

The application expects a Moxfield-style decklist file named `cards.txt`. Each line follows this format:

```
<quantity> <card name> (<set code>) <collector number>[ *F* or *E*]
```

**Components:**
- `<quantity>`: Number of copies (e.g., `1`, `9`)
- `<card name>`: Card name (e.g., `Mountain`, `Gogo, Mysterious Mime`)
- `<set code>`: Set code in parentheses (e.g., `(FIN)`, `(FIC)`)
- `<collector number>`: Collector number (e.g., `303`, `153a`)
- `[ *F*]`: Optional foil marker
- `[ *E*]`: Optional etched marker

**Examples:**

```
1 Gogo, Mysterious Mime (FIC) 153 *F*        # Foil version
1 Drakuseth, Maw of Flames (CMM) 535 *E*     # Etched version
9 Mountain (FIN) 303                          # Non-foil (default)
```

**Notes:**
- Lines starting with `@` are treated as sideboard entries and skipped
- Cards without a finish marker default to non-foil
- The application filters for English language cards only

### Output Example

```
1x Gogo, Mysterious Mime - €2.50
1x Access Tunnel - €3.00
9x Mountain - €4.50

Cart summary: 3 items added, 0 failed
Total price: €10.00

Add to cart? (y/n): y

✓ Added: 1x Gogo, Mysterious Mime
✓ Added: 1x Access Tunnel
✓ Added: 9x Mountain

Cart summary: 3 items added, 0 failed
```

## Project Structure

```
ScryTrader/
├── Configuration/
│   └── CardTraderOptions.cs          # Configuration POCOs for appsettings binding
├── Models/                           # Data models for cards, products, cart, etc.
│   ├── Blueprint.cs                  # Card Trader blueprint model
│   ├── CardFinish.cs                 # Enum: NonFoil, Foil, Etched
│   ├── CardCondition.cs            # Enum: NearMint, SlightlyPlayed, etc.
│   ├── DeckCard.cs                   # Parsed deck card entry
│   ├── MarketplaceProduct.cs       # Marketplace listing model
│   ├── ProductSelection.cs         # Price calculation result
│   └── CartSummary.cs              # Cart display model with Print() method
├── Services/                         # Core application logic
│   ├── CardTraderClient.cs         # HTTP client for Card Trader API v2
│   ├── ScryfallClient.cs           # HTTP client for Scryfall API
│   ├── BlueprintMatcher.cs         # Maps deck cards to Card Trader blueprints
│   ├── DeckPriceCalculator.cs      # Orchestrates price lookup pipeline
│   └── MoxfieldDeckParser.cs       # Parses Moxfield decklist format
├── appsettings.json                  # Runtime configuration (not tracked in git)
├── appsettings.example.json        # Template showing required config structure
├── cards.txt                         # Sample decklist file
├── ScryTraderApplication.cs        # Main application orchestrator
└── Program.cs                        # Entry point
```

## Technical Details

- **Framework**: .NET 10.0 (net10.0)
- **Publishing**: Self-contained, single-file, win-x64 runtime identifier
- **APIs Used**:
  - Card Trader API v2 (`api.cardtrader.com`) - marketplace products, blueprints, cart management
  - Scryfall API (`api.scryfall.com`) - card database and pricing information
- **Caching**: Internal caches for blueprints and marketplace products to reduce API calls
- **CI/CD**: GitHub Actions workflow publishes self-contained binaries on tag creation

## License

This project is for personal use.
