using DataManager.Constant;
using DataManager.Constant.Enums;
using DataManager.DesignPattern.Builder;
using DataManager.Helper.Extension;
using DataManager.Helper.Utility;
using OpenQA.Selenium;

namespace DataManager.Handler;
public class UnlikeAllPostsHandler : BaseCommandHandler
{
    private const string OperationPath = "https://www.instagram.com/your_activity/interactions/likes/";
    private const string ImageXPath = "//img[@data-bloks-name='bk.components.Image']";
    private const string UnlikeButtonXPath = "//*[name()='svg' and @role='img' and (@aria-label='Unlike' or @aria-label='Like') and (contains(@class, 'xyb1xck') or contains(@class, 'xxk16z8'))]";
    private const string ErrorRefreshImageSource = "https://i.instagram.com/static/images/bloks/assets/ig_illustrations/illo_error_refresh-4x-dark.png/02ffe4dfdf20.png";

    private int _unlikedCount;
    private readonly Dictionary<string, int> _visitedPosts = [];
    private readonly HashSet<string> _blackList = [];

    public override OperationType OperationType => OperationType.SeleniumBased;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");

        var taskBuilder = new SeleniumTaskBuilder(webDriver);
        taskBuilder
            .NavigateTo(OperationPath)
            .PerformAction(HandleAllPosts)
            .ExecuteTasks();
    }

    private void HandleAllPosts(IWebDriver webDriver)
    {
        "Starting the process of unliking posts...".WriteMessage(MessageType.Info);
        WebDriverExtension.EnsureDomLoaded(webDriver);

        while (true)
        {
            try
            {
                if (!TryProcessNextPost(webDriver))
                {
                    "No more posts found. Exiting...".WriteMessage(MessageType.Warning);
                    break;
                }
            }
            catch (NoSuchElementException ex)
            {
                $"Element not found during post processing: {ex.Message}".WriteMessage(MessageType.Warning);
                break;
            }
            catch (Exception ex)
            {
                $"Unexpected error during post processing: {ex.Message}".WriteMessage(MessageType.Error);
                break;
            }
        }
    }

    private bool TryProcessNextPost(IWebDriver webDriver)
    {
        By by = By.XPath(ImageXPath);
        IWebElement webElement;

        try
        {
            if (_blackList.Count != 0)
            {
                var elements = webDriver.FindElements(by).Take(_blackList.Count + 1).ToList();
                if (elements.Count <= _blackList.Count)
                    return false;

                webElement = elements[_blackList.Count];
            }
            else
            {
                webElement = webDriver.FindElementWithRetries("Liked Post", by, 3, 1500) ??
                    throw new InvalidOperationException("Failed to find the element after retries.");
            }

            string srcValue = webElement.GetDomAttribute("src");
            string? url = GetBaseUrl(srcValue);

            if (string.Equals(url, ErrorRefreshImageSource, StringComparison.OrdinalIgnoreCase))
                return false;

            if (AddToVisitedPosts(url))
            {
                ScrollToElement(webDriver, webElement);
                OpenAndUnlikePost(webDriver, webElement, url);
                return true;
            }

            return false;
        }
        catch (NoSuchElementException ex)
        {
            ex.LogException("Element not found");
            return false;
        }
        catch (Exception ex)
        {
            ex.LogException("An error occurred while processing the post");
            return false;
        }
    }

    private string GetBaseUrl(string url)
    {
        int indexOfGid = url.IndexOf("gid");
        return url[..indexOfGid];
    }

    private bool AddToVisitedPosts(string srcValue)
    {
        try
        {
            if (_visitedPosts.TryGetValue(srcValue, out int retryCount))
            {
                retryCount++;
                if (retryCount > AppConstant.MaxRetryPerPost)
                {
                    $"Post '{srcValue}' reached max retry limit ({AppConstant.MaxRetryPerPost}). Skipping.".WriteMessage(MessageType.Warning);
                    return false;
                }

                _visitedPosts[srcValue] = retryCount;
                $"Post '{srcValue}' already visited. Retry #{retryCount}.".WriteMessage(MessageType.Error);
            }
            else
            {
                _visitedPosts[srcValue] = 0;
            }

            return true;
        }
        catch (Exception ex)
        {
            ex.LogException($"Error processing post identifier '{srcValue}'");
            return false;
        }
    }

    private void ScrollToElement(IWebDriver webDriver, IWebElement webElement)
    {
        ((IJavaScriptExecutor)webDriver).ExecuteScript("arguments[0].scrollIntoView(true);", webElement);
        webDriver.WaitForElementVisible(webElement);
    }

    private void OpenAndUnlikePost(IWebDriver webDriver, IWebElement postElement, string src)
    {
        try
        {
            postElement.Click();
            WebDriverExtension.EnsureDomLoaded(webDriver);

            IWebElement? iconElement = webDriver.FindElementWithRetries("Unlike Button", By.XPath(UnlikeButtonXPath));
            if (iconElement != null)
            {
                //ConsoleExtension.ClearLine();

                string ariaLabel = iconElement.GetDomAttribute("aria-label");
                if (ariaLabel == "Unlike")
                {
                    iconElement.Click();
                    $"Successfully unliked post #{++_unlikedCount}.".WriteMessage(MessageType.Success);
                }
            }
            else
            {
                $"Unlike button not found for post #{_unlikedCount}".WriteMessage(MessageType.Warning);
                _blackList.Add(src);
            }
        }
        catch (Exception ex)
        {
            $"Unexpected error while unliking post #{_unlikedCount}: {ex.Message}".WriteMessage(MessageType.Error);
        }
        finally
        {
            webDriver.Navigate().Back();
            WebDriverExtension.EnsureDomLoaded(webDriver);
        }
    }
}