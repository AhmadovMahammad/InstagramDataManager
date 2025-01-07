using DataManager.Constant;
using DataManager.Constant.Enums;
using DataManager.DesignPattern.Builder;
using DataManager.Handler;
using DataManager.Helper.Extension;
using DataManager.Helper.Utility;
using OpenQA.Selenium;

public class UnlikePostsHandler : BaseCommandHandler
{
    private const string OperationPath = "https://www.instagram.com/your_activity/interactions/likes/";
    private const string PostXPath = "//img[@data-bloks-name='bk.components.Image']";
    private const string UnlikeButtonXPath = "//*[name()='svg' and @role='img' and (@aria-label='Unlike' or @aria-label='Like') and (contains(@class, 'xyb1xck') or contains(@class, 'xxk16z8'))]";
    private const string ErrorRefreshImageSource = "https://i.instagram.com/static/images/bloks/assets/ig_illustrations/illo_error_refresh-4x-dark.png/02ffe4dfdf20.png";
    private static readonly string ErrorRefreshImageXPath = $"//img[@data-bloks-name='bk.components.Image' and @src='{ErrorRefreshImageSource}']";

    private readonly Dictionary<string, int> _visitedPosts = [];
    private readonly HashSet<string> _blackList = [];
    private int _unlikeCount = 0;

    public override OperationType OperationType => OperationType.SeleniumBased;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");

        var taskBuilder = new SeleniumTaskBuilder(webDriver);
        taskBuilder
            .NavigateTo(OperationPath)
            .PerformAction(HandlePosts)
            .ExecuteTasks();
    }

    private void HandlePosts(IWebDriver webDriver)
    {
        "Starting the process of unliking posts...".WriteMessage(MessageType.Info);

        // First, look for ErrorRefreshImageXPath; if it's present, you haven't liked any postings.
        IWebElement? webElement = webDriver.FindElementWithRetries("Error Image", By.XPath(ErrorRefreshImageXPath), 1, 1000, false);
        if (webElement != null)
        {
            "You haven’t liked anything. Exiting...".WriteMessage(MessageType.Warning);
            return;
        }

        while (true)
        {
            WebDriverExtension.EnsureDomLoaded(webDriver);

            try
            {
                PostProcessType postProcessType = TryProcessNextPost(webDriver);
                string consoleMessage = postProcessType switch
                {
                    PostProcessType.NoMorePosts => "No more posts found. Exiting...",
                    PostProcessType.MaxRetryLimitReached => "Reached max retry limit for same post. Exiting...",
                    _ => string.Empty
                };

                if (postProcessType != PostProcessType.Success)
                {
                    consoleMessage.WriteMessage(MessageType.Warning);
                    break;
                }
            }
            catch (Exception ex)
            {
                ex.LogException("Unexpected error during Post processing.");
                break;
            }
        }
    }

    private PostProcessType TryProcessNextPost(IWebDriver webDriver)
    {
        By by = By.XPath(PostXPath);
        string sourceValue = string.Empty;
        IWebElement? webElement = null;

        try
        {
            // avoid prohibited posts:
            // Occasionally, Instagram posts that you like cannot be opened due to account blocking or other issues.
            if (_blackList.Count != 0)
            {
                var elements = webDriver.FindElements(by).Skip(_blackList.Count).ToList();
                if (elements.Count <= _blackList.Count)
                {
                    return PostProcessType.NoMorePosts;
                }

                webElement = elements.First();
            }
            else
            {
                webElement = webDriver.FindElementWithRetries("Liked Post", by, 3, 1500);
                if (webElement != null)
                {
                    sourceValue = webElement.GetDomAttribute("src");
                    if (sourceValue == ErrorRefreshImageSource)
                    {
                        return PostProcessType.NoMorePosts;
                    }
                }
            }

            if (webElement is null) return PostProcessType.NoMorePosts;
            if (TryAdd2Visited(webElement, sourceValue, out bool maxLimitReached))
            {
                if (maxLimitReached)
                {
                    return PostProcessType.MaxRetryLimitReached;
                }

                webDriver.ScrollToElement(webElement);
                OpenAndUnlikePost(webDriver, webElement, sourceValue);
            }

            return PostProcessType.Success;
        }
        catch (Exception ex)
        {
            ex.LogException("An error occurred while processing the post");
            return PostProcessType.Error;
        }
    }

    private bool TryAdd2Visited(IWebElement webElement, string sourceValue, out bool maxLimitReached)
    {
        maxLimitReached = false;

        try
        {
            if (_visitedPosts.TryGetValue(sourceValue, out int retryCount) && retryCount > 0)
            {
                retryCount++;
                if (retryCount > AppConstant.MaxRetryPerPost)
                {
                    maxLimitReached = true;
                    $"Post reached max retry limit ({AppConstant.MaxRetryPerPost}). Skipping.".WriteMessage(MessageType.Warning);
                }

                return false;
            }
            else
            {
                _visitedPosts.Add(sourceValue, 1);
            }

            return true;
        }
        catch (Exception ex)
        {
            ex.LogException($"Error processing post identifier.");
            return false;
        }
    }

    private void OpenAndUnlikePost(IWebDriver webDriver, IWebElement webElement, string sourceValue)
    {
        try
        {
            webElement.Click();
            WebDriverExtension.EnsureDomLoaded(webDriver);

            IWebElement? iconElement = webDriver.FindElementWithRetries("Unlike Button", By.XPath(UnlikeButtonXPath), 1, initialDelay: 1500, logMessage: false);
            if (iconElement != null)
            {
                string ariaLabel = iconElement.GetDomAttribute("aria-label");
                if (ariaLabel == "Unlike")
                {
                    iconElement.Click();
                    _unlikeCount++;

                    $"Successfully unliked post #{_unlikeCount}.".WriteMessage(MessageType.Success);
                }
            }
            else
            {
                $"Unlike button not found for post #{_unlikeCount}. Adding to BlackList.".WriteMessage(MessageType.Warning);
                _blackList.Add(sourceValue);
            }
        }
        catch (Exception ex)
        {
            $"Unexpected error while unliking post #{_unlikeCount}: {ex.Message}".WriteMessage(MessageType.Error);
        }
        finally
        {
            webDriver.Navigate().Back();
            WebDriverExtension.EnsureDomLoaded(webDriver);
        }
    }
}