using DataManager.Constants.Enums;
using DataManager.DesignPatterns.StrategyDP.Contracts;
using DataManager.Handlers;

namespace DataManager;
public interface IOperationHandler
{
    void Execute(IFileFormatStrategy fileFormatStrategy);
}

public static class OperationFactory
{
    public static IOperationHandler? CreateHandler(OperationType operationType, string filePath)
    {
        // If we don't support this operation, just return null.
        return operationType switch
        {
            OperationType.Display_Recent_Follow_Requests => new DisplayRecentFollowRequestsHandler(filePath),
            OperationType.Display_Received_Requests => new DisplayReceivedRequestsHandler(filePath),
            OperationType.Display_Blocked_Profiles => new DisplayBlockedProfilesHandler(filePath),
            OperationType.Display_Close_Friends => new DisplayCloseFriendsHandler(filePath),
            OperationType.Display_Followers => new DisplayFollowersHandler(filePath),
            OperationType.Display_Following => new DisplayFollowingHandler(filePath),
            OperationType.Display_Recently_Unfollowed => new DisplayRecentlyUnfollowedHandler(filePath),
            OperationType.Display_Pending_Follow_Requests => new DisplayPendingFollowRequestsHandler(filePath),
            _ => null
        };
    }
}
