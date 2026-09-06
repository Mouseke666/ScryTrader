namespace ScryTrader.Models;

public class CartAddResult
{
    public bool Success { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public string? ErrorMessage { get; set; }
}
