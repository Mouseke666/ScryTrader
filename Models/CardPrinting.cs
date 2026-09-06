namespace ScryTrader.Models;

public class CardPrinting
{
    public string Name { get; set; } = string.Empty;

    public string SetCode { get; set; } = string.Empty;

    public string CollectorNumber { get; set; } = string.Empty;

    public CardFinish Finish { get; set; }
    public bool IsSpecial { get; set; }

}