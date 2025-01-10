using DataManager.Constant.Enums;
using DataManager.Handler;
using DataManager.Handler.ManageHandlers;
using OpenQA.Selenium;

namespace DataManager.Factory;
public interface ICommandHandler
{
    void HandleCommand(IWebDriver webDriver);
}

public static class CommandFactory
{
    public static ICommandHandler? CreateHandler(CommandType commandType)
    {
        return commandType switch
        {
            CommandType.Manage_Followers => new ManageFollowersHandler(),
            CommandType.Manage_Recently_Unfollowed => new ManageRecentlyUnfollowedHandler(),
            CommandType.Unlike_All_Posts => new UnlikePostsHandler(),
            CommandType.Manage_Received_Requests => new ManageReceivedRequestsHandler(),
            CommandType.Manage_Blocked_Profiles => new ManageBlockedProfilesHandler(),
            CommandType.Manage_Close_Friends => new ManageCloseFriendsHandler(),
            var type when (type == CommandType.Manage_Pending_Follow_Requests || type == CommandType.Manage_Recent_Follow_Requests)
            => new ManageFollowRequestsHandler(commandType),

            _ => null
        };
    }
}
