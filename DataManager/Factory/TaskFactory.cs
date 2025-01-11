using DataManager.Constant.Enums;
using DataManager.Tasks;
using OpenQA.Selenium;

namespace DataManager.Factory;
public interface ITaskHandler
{
    void HandleTask(IWebDriver webDriver);
}

public static class TaskFactory
{
    public static ITaskHandler? CreateTask(CommandType commandType)
    {
        return commandType switch
        {
            CommandType.Manage_Followers => new ManageFollowers(),
            CommandType.Manage_Recently_Unfollowed => new ManageUnfollowedProfiles(),
            CommandType.Unlike_All_Posts => new UnlikePosts(),
            CommandType.Manage_Received_Requests => new ManageReceivedRequests(),
            CommandType.Manage_Blocked_Profiles => new ManageBlockedProfiles(),
            CommandType.Manage_Close_Friends => new ManageCloseFriends(),
            var cType when (cType == CommandType.Manage_Pending_Follow_Requests || cType == CommandType.Manage_Recent_Follow_Requests)
            => new ManageFollowRequests(commandType),

            _ => null
        };
    }
}
