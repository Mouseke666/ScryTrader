namespace ScryTrader.Models;

public class ProductSelection
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal TotalPrice { get; set; }
    public string CardName { get; set; } = "";
    public CardFinish Finish { get; set; }
}
