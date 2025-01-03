namespace DataManager.Model;
public record DataModel
{
    public DataModel(long timestamp)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        Date = dateTime.AddSeconds(timestamp).ToShortDateString();
    }

    public string Title { get; init; } = string.Empty;
    public string Href { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Date { get; } = string.Empty;
}
