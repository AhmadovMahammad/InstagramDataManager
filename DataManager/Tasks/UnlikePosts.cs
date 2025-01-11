using DataManager.Constant;
using DataManager.Constant.Enums;
using DataManager.DesignPattern.Builder;
using DataManager.Helper.Extension;
using DataManager.Helper.Utility;
using OpenQA.Selenium;

namespace DataManager.Tasks;
public class UnlikePosts : BaseTaskHandler
{
    private const string _operationPath = "https://www.instagram.com/your_activity/interactions/likes/";

    private readonly Dictionary<string, int> _visitedPosts = [];
    private readonly HashSet<string> _blackList = [];
    private int _unlikeCount = 0;

    public override OperationType OperationType => OperationType.SeleniumBased;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");

        var taskBuilder = new SeleniumTaskBuilder(webDriver);
        taskBuilder
            .NavigateTo(_operationPath)
            .PerformAction(HandlePosts)
            .ExecuteTasks();
    }

    private void HandlePosts(IWebDriver webDriver)
    {
        "Starting the process of unliking posts...".WriteMessage(MessageType.Info);

        // First, look for ErrorRefreshImageXPath; if it's present, you haven't liked any postings.
        IWebElement? webElement = webDriver.FindWebElement(By.XPath(XPathConstants.ErrorRefreshImageXPath), WebElementPriorityType.Low);
        if (webElement != null)
        {
            "You haven’t liked anything. Exiting...".WriteMessage(MessageType.Warning);
            return;
        }

        while (true)
        {
            webDriver.EnsureDomLoaded();

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
        By by = By.XPath(XPathConstants.PostImage);
        string sourceValue = string.Empty;
        IWebElement? webElement = null;

        try
        {
            // avoid prohibited posts:
            // Occasionally, Instagram posts that you like cannot be opened due to account blocking or other issues.
            if (_blackList.Count > 0)
            {
                var elements = webDriver.FindElements(by)
                                        .Skip(_blackList.Count)
                                        .Where(webElement => webElement.GetDomAttribute("src") != XPathConstants.ErrorRefreshImageSource);

                if (!elements.Skip(_blackList.Count).Any())
                {
                    return PostProcessType.NoMorePosts;
                }

                webElement = elements.FirstOrDefault();
            }
            else webElement = webDriver.FindWebElement(by, WebElementPriorityType.Low);

            if (webElement == null) return PostProcessType.NoMorePosts;
            sourceValue = webElement.GetDomAttribute("src");
            
            if (webElement.GetDomAttribute("src") == XPathConstants.ErrorRefreshImageSource)
            {
                return PostProcessType.NoMorePosts;
            }

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
            webDriver.EnsureDomLoaded();

            // TODO: Check for error refresh image at first

            IWebElement? iconElement = webDriver.FindWebElement(By.XPath(XPathConstants.UnlikeButton), WebElementPriorityType.Medium);
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
        }
    }
}