namespace ScryTrader.Services;

public class CardTraderApiException : Exception
{
    public CardTraderApiException(string message)
        : base(message)
    {
    }

    public CardTraderApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}