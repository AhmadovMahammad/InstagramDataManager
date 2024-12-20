﻿using DataManager.Constants.Enums;
using DataManager.Handlers;
using DataManager.Handlers.DisplayHandlers;

namespace DataManager.Factories;
public interface IOperationHandler
{
    void HandleOperation();
}

public static class OperationFactory
{
    public static IOperationHandler? CreateHandler(OperationType operationType)
    {
        return operationType switch
        {
            OperationType.Display_Recent_Follow_Requests => new DisplayRecentFollowRequestsHandler(),
            OperationType.Display_Received_Requests => new DisplayReceivedRequestsHandler(),
            OperationType.Display_Blocked_Profiles => new DisplayBlockedProfilesHandler(),
            OperationType.Display_Close_Friends => new DisplayCloseFriendsHandler(),
            OperationType.Display_Followers => new DisplayFollowersHandler(),
            OperationType.Display_Following => new DisplayFollowingHandler(),
            OperationType.Display_Recently_Unfollowed => new DisplayRecentlyUnfollowedHandler(),
            OperationType.Display_Pending_Follow_Requests => new DisplayPendingFollowRequestsHandler(),
            OperationType.Unlike_All_Posts => new UnlikeAllPostsHandler(),
            _ => null
        };
    }
}
