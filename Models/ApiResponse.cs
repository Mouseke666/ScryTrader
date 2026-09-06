namespace ScryTrader.Models;

public class ApiResponse<T> where T : notnull
{
    public T Array { get; set; } = default!;
}
