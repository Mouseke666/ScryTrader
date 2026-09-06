namespace ScryTrader.Models;

public static class CardConditionExtensions
{
    public static string ToCardTraderValue(this CardCondition condition)
    {
        return condition switch
        {
            CardCondition.NearMint => "Near Mint",
            CardCondition.SlightlyPlayed => "Slightly Played",
            CardCondition.ModeratelyPlayed => "Moderately Played",
            CardCondition.Played => "Played",
            CardCondition.Poor => "Poor",
            _ => throw new ArgumentOutOfRangeException(nameof(condition), condition, null)
        };
    }
}