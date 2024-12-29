using DataManager.Constants;
using DataManager.Constants.Enums;
using DataManager.DesignPatterns.Builder;
using DataManager.Helpers.Extensions;
using DataManager.Helpers.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DataManager.Handlers;
public class UnlikeAllPostsHandler : BaseCommandHandler
{
    private const string OperationPath = "https://www.instagram.com/your_activity/interactions/likes/";
    private const string ImageXPath = "//img[@data-bloks-name='bk.components.Image']";
    private const string UnlikeButtonXPath = "//*[name()='svg' and @role='img' and (@aria-label='Unlike' or @aria-label='Like') and (contains(@class, 'xyb1xck') or contains(@class, 'xxk16z8'))]";

    private int _unlikedCount;
    private readonly Dictionary<string, int> _visitedPosts = [];
    private readonly HashSet<string> _blackList = [];

    public override OperationType OperationType => OperationType.SeleniumBased;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");

        var taskBuilder = new SeleniumTaskBuilder(webDriver);
        BuildUnlikePostTask(taskBuilder).ExecuteTasks();
    }

    private ITaskBuilder BuildUnlikePostTask(ITaskBuilder builder)
    {
        return builder
            .NavigateTo(OperationPath)
            .PerformAction(HandleAllPosts);
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
        IWebElement webElement = null!;

        if (_blackList.Count != 0)
        {
            int blacklistLength = _blackList.Count;

            IEnumerable<IWebElement> elements = webDriver.FindElements(by).Take(blacklistLength + 1);
            webElement = elements.Skip(blacklistLength).First();
        }
        else
        {
            webElement = FindElementWithRetries(webDriver, "Liked Post", by, 3, 1500)!;
        }

        string? src = GetBaseUrl(webElement.GetDomAttribute("src"));
        if (AddToVisitedPosts(src))
        {
            ScrollToElement(webDriver, webElement);
            OpenAndUnlikePost(webDriver, webElement, src);

            return true;
        }

        return false;
    }

    private string GetBaseUrl(string url)
    {
        Uri uri = new Uri(url);
        return uri.GetLeftPart(UriPartial.Path);
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
                $"Post '{srcValue}' already visited. Retry #{retryCount}.".WriteMessage(MessageType.Warning);
            }
            else
            {
                _visitedPosts[srcValue] = 1;
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
        WaitForElementVisible(webDriver, webElement);
    }

    private void OpenAndUnlikePost(IWebDriver webDriver, IWebElement postElement, string src)
    {
        try
        {
            postElement.Click();
            WebDriverExtension.EnsureDomLoaded(webDriver);

            IWebElement? iconElement = FindElementWithRetries(webDriver, "Unlike Button", By.XPath(UnlikeButtonXPath));
            if (iconElement != null)
            {
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

    private IWebElement? FindElementWithRetries(IWebDriver webDriver, string elementName, By by, int retries = 3, int initialDelay = 1500)
    {
        for (int attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                return webDriver.FindElement(by);
            }
            catch (NoSuchElementException) when (attempt < retries)
            {
                $"Retrying to find webElement: '{elementName}'. [{attempt}/{retries}]".WriteMessage(MessageType.Warning);

                int currentDelay = initialDelay / attempt;
                Task.Delay(currentDelay).Wait();
            }
            catch (Exception)
            {
                $"Error finding webElement: '{elementName}'".WriteMessage(MessageType.Error);
                break;
            }
        }

        return null;
    }

    private void WaitForElementVisible(IWebDriver webDriver, IWebElement webElement, int timeoutSeconds = 10)
    {
        new WebDriverWait(webDriver, TimeSpan.FromSeconds(timeoutSeconds))
            .Until(d => webElement.Displayed);
    }
}