namespace DataManager.Model;
public record FollowAnalyzeData
{
    public string NewFollower { get; init; } = string.Empty;
    public string NewFollowing { get; init; } = string.Empty;
    public string RemovedFollower { get; init; } = string.Empty;
    public string RemovedFollowing { get; init; } = string.Empty;
}
