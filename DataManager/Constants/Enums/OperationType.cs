using System.ComponentModel;

namespace DataManager.Constants.Enums;
public enum OperationType
{
    [Description("Displays the most recent follow requests.")]
    Display_Recent_Follow_Requests = 1,

    [Description("Displays all the follow requests you have received.")]
    Display_Received_Requests,

    [Description("Displays a list of profiles you've blocked.")]
    Display_Blocked_Profiles,

    [Description("Displays your close friends.")]
    Display_Close_Friends,

    [Description("Displays your followers.")]
    Display_Followers,

    [Description("Displays the profiles you're following.")]
    Display_Following,

    [Description("Displays users you have recently unfollowed.")]
    Display_Recently_Unfollowed,

    [Description("Displays follow requests that are still pending.")]
    Display_Pending_Follow_Requests,

    [Description("Unfollows all profiles that do not follow you back.")]
    Unfollow_Non_Followers,

    [Description("Unfollows all profiles you have sent follow requests to.")]
    Unfollow_Sent_Follow_Requests,

    [Description("Unblocks all the profiles you have previously blocked.")]
    Unblock_All_Blocked_Profiles,

    [Description("Unlikes all posts you have liked.")]
    Unlike_All_Posts
}