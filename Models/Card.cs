namespace ScryTrader.Models;

public class Card
{
    public string Name { get; set; } = string.Empty;

    public List<CardPrinting> Printings { get; set; } = [];
}