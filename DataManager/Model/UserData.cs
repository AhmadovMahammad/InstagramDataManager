namespace DataManager.Model;
public record UserData(HashSet<UserEntry> Followers, HashSet<UserEntry> Following, DateTime LastChecked)
{
    public UserData() : this([], [], DateTime.Now)
    {

    }
}

public record UserEntry
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
};