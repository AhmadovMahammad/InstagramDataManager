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
            // todo: merge 'manage recent/pending follow requests'
            CommandType.Manage_Recent_Follow_Requests => new ManageRecentFollowRequestsHandler(),
            CommandType.Manage_Received_Requests => new ManageReceivedRequestsHandler(),
            CommandType.Manage_Blocked_Profiles => new ManageBlockedProfilesHandler(),
            CommandType.Manage_Close_Friends => new ManageCloseFriendsHandler(),
            CommandType.Manage_Followers => new ManageFollowersHandler(),
            CommandType.Manage_Recently_Unfollowed => new ManageRecentlyUnfollowedHandler(),
            CommandType.Manage_Pending_Follow_Requests => new ManagePendingFollowRequestsHandler(),
            CommandType.Unlike_All_Posts => new UnlikePostsHandler(),
            _ => null
        };
    }
}
