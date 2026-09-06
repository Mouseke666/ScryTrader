using ScryTrader.Models;

namespace ScryTrader.Configuration;

public class CardTraderOptions
{
    public string AuthToken { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.cardtrader.com/api/v2/";
    
    public Address? ShippingAddress { get; set; }
    public Address? BillingAddress { get; set; }
}