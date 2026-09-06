namespace ScryTrader.Models;

public class DeckCard
{
    public int Quantity { get; set; }

    public CardPrinting Printing { get; set; } = null!;
}