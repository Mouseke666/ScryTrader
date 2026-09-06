namespace ScryTrader.Models;

public class CartSummary
{
    public List<ProductSelection> Items { get; set; } = new();
    public decimal TotalPrice { get; set; }
    
    public void Print()
    {
        Console.WriteLine("\n=== Cart Summary ===");
        Console.WriteLine($"Total items: {Items.Sum(i => i.Quantity)}");
        Console.WriteLine($"Total price: €{TotalPrice:F2}\n");
        
        foreach (var item in Items)
        {
            Console.WriteLine($"{item.CardName} ({item.Finish}) - {item.Quantity}x €{item.PricePerUnit:F2} = €{item.TotalPrice:F2}");
        }
    }
}
