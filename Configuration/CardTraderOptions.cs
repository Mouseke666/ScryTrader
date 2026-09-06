namespace ScryTrader.Configuration;

public class CardTraderOptions
{
    public string AuthToken { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.cardtrader.com/api/v2/";
}