using System.ComponentModel;

namespace DataManager.Constant.Enums;
public enum CommandType
{
    [Description("Provides instructions on how to get JSON or HTML files from Instagram.")]
    Get_Info = 1,

    [Description("Manage your followers.")]
    Manage_Followers,

    [Description("Manage recently unfollowed users.")]
    Manage_Recently_Unfollowed,

    [Description("Unlikes all posts you have liked.")]
    Unlike_All_Posts,

    [Description("Manage follow requests.")]
    Manage_Recent_Follow_Requests,

    [Description("Manage follow requests that are still pending.")]
    Manage_Pending_Follow_Requests,

    [Description("Manage all received follow requests.")]
    Manage_Received_Requests,

    [Description("Manage profiles you've blocked.")]
    Manage_Blocked_Profiles,

    [Description("Manage your close friends list.")]
    Manage_Close_Friends,

    [Description("Makes extra room for the user by clearing the console.")]
    Clear_Console,
}