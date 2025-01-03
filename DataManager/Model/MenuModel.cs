namespace DataManager.Model;
public record MenuModel
{
    public int Key { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
