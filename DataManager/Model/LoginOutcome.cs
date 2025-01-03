namespace DataManager.Model;
public record LoginOutcome
{
    public LoginOutcome(string condition, object? data = null)
    {
        Condition = condition;
        Data = data;
    }

    public string Condition { get; init; } = string.Empty;
    public object? Data { get; init; }

    public static readonly LoginOutcome Empty = new(string.Empty);
}
