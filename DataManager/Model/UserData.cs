namespace DataManager.Model;
public record UserData(HashSet<UserEntry> Followers, HashSet<UserEntry> Following, DateTime LastChecked)
{
    public UserData() : this([], [], DateTime.Now)
    {

    }
}

public record UserEntry
{
    public string Identifier {  get; init; } = string.Empty;
    public string Username {  get; init; } = string.Empty;
    public string Fullname {  get; init; } = string.Empty;
};