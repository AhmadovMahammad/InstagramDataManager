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
    private const string ErrorTextXPath = "//span[contains(text(),'Something went wrong')]";

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
            webElement = webDriver.FindElement(by);
        }

        string? src = webElement.GetDomAttribute("src");
        if (AddToVisitedPosts(src))
        {
            ScrollToElement(webDriver, webElement);
            OpenAndUnlikePost(webDriver, webElement, src);
        }

        return true;
    }

    private bool AddToVisitedPosts(string srcValue)
    {
        try
        {
            if (_visitedPosts.TryGetValue(srcValue, out int value))
            {
                if (value >= AppConstant.MaxRetryPerPost)
                    throw new InvalidOperationException($"Post reached max retry limit ({AppConstant.MaxRetryPerPost}).");

                _visitedPosts[srcValue] = ++value;
                $"Post already visited. Retry #{value}.".WriteMessage(MessageType.Warning);
                return false;
            }

            _visitedPosts[srcValue] = 0;
            return true;
        }
        catch (Exception ex)
        {
            $"Error processing post identifier: {ex.Message}".WriteMessage(MessageType.Warning);
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

            // Check for "Something went wrong" message at first
            bool somethingWentWrong = webDriver.FindElements(By.XPath("//span[contains(text(),'Something went wrong')]")).Count != 0;
            if (somethingWentWrong)
            {
                $"Post could not be opened, adding to blacklist.".WriteMessage(MessageType.Warning);
                _blackList.Add(webDriver.Url);
                return;
            }

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

    private IWebElement? FindElementWithRetries(IWebDriver webDriver, string elementName, By by, int retries = 3)
    {
        for (int attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                return webDriver.FindElement(by);
            }
            catch (NoSuchElementException) when (attempt < retries)
            {
                $"Retrying to find webElement: '{elementName}', ({attempt}/{retries})...".WriteMessage(MessageType.Warning);
                Task.Delay(1000).Wait();
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