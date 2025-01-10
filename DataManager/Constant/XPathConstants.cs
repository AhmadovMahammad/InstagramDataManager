namespace DataManager.Constant;
public static class XPathConstants
{
    // SeleniumAutomation (Login)
    public const string UsernameField = "//input[@name='username']";
    public const string PasswordField = "//input[@name='password']";
    public const string SubmitButton = "//button[@type='submit']";
    public const string PasswordIncorrectLabel = "//div[contains(text(),'your password was incorrect')]";
    public const string SaveInfoButton = "//button[text()='Save info' and contains(@class,'_acan _acap _acas _aj1- _ap30')]";

    // ManageFollowersHandler
    public const string ManagableUsername = "//span[@class='_ap3a _aaco _aacw _aacx _aad7 _aade' and @dir='auto']";
    public const string FollowButton = "//div[@dir='auto' and normalize-space(text())='Follow']";
    public const string NotAvailableNotification = "//a[@role='link' and normalize-space(text()) = 'Go back to Instagram.']";
    public const string Scrollbar = "//div[contains(@class,'xyi19xy x1ccrb07 xtf3nb5 x1pc53ja x1lliihq x1iyjqo2 xs83m0k xz65tgg x1rife3k x1n2onr6')]";

    // Manage[Pending/Recent]FollowRequestsHandler
    public const string RequestedButton = "//button[contains(@class,'_acan') and ..//div[text()='Requested']]";
    public const string UnfollowButton = "//button[contains(@class, '_a9-- _ap36 _a9-_') and normalize-space(text())='Unfollow']";

    // ManageReceivedRequestsHandler
    public const string NotificationsSidebarButton = "//*[name()='svg' and @aria-label='Notifications']";
    public const string ExpandNotificationsButton = "//*[name()='svg' and @aria-label='' and contains(@class, 'x1lliihq x1n2onr6 x1roi4f4')]";
    public const string ConfirmButton = "//div[contains(text(), 'Confirm')]";
    public const string DeleteButton = "//div[contains(text(), 'Delete')]";

    // UnlikePostsHandler
    public const string OperationPath = "https://www.instagram.com/your_activity/interactions/likes/";
    public const string ErrorRefreshImageSource = "https://i.instagram.com/static/images/bloks/assets/ig_illustrations/illo_error_refresh-4x-dark.png/02ffe4dfdf20.png";
    public const string PostImage = "//img[@data-bloks-name='bk.components.Image']";
    public const string UnlikeButton = "//*[name()='svg' and @role='img' and (@aria-label='Unlike' or @aria-label='Like') and (contains(@class, 'xyb1xck') or contains(@class, 'xxk16z8'))]";
    public const string ErrorRefreshImageXPath = $"//img[@data-bloks-name='bk.components.Image' and @src='{ErrorRefreshImageSource}']";

    // ManageBlockedProfilesHandler
    public const string UnblockButton = "//button[contains(@class, '_acan') and contains(@class, '_acap')//div[contains(text(), 'Unblock')]";
}