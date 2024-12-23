using DataManager.Constants;
using DataManager.Constants.Enums;
using DataManager.DesignPatterns.Builder;
using DataManager.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DataManager.Handlers;
public class UnlikeAllPostsHandler : BaseOperationHandler
{
    private readonly string _operationPath = @"https://www.instagram.com/your_activity/interactions/likes/";
    private int _unlikedCount;
    private readonly HashSet<string> _visitedPosts = [];
    private readonly Dictionary<string, int> _retryCounts = [];
    private const int MaxRetryPerPost = 3;

    public override bool RequiresFile => false;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        if (!parameters.TryGetValue("WebDriver", out var driverObj) || driverObj is not IWebDriver driver)
            throw new ArgumentException("Missing or invalid WebDriver in parameters.");

        var taskBuilder = new SeleniumTaskBuilder(driver);
        BuildUnlikePostTask(taskBuilder).ExecuteTasks();
    }

    private ITaskBuilder BuildUnlikePostTask(ITaskBuilder builder)
    {
        return builder
            .NavigateTo(_operationPath)
            .PerformAction(d => d.Manage().Window.Maximize())
            .PerformAction(HandleAllPosts)
            .PerformAction(driver => driver.Quit());
    }

    private void HandleAllPosts(IWebDriver driver)
    {
        "Starting the process of unliking posts...".WriteMessage(MessageType.Info);
        EnsureDomLoaded(driver);

        while (true)
        {
            try
            {
                if (!TryProcessNextPost(driver))
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

    private bool TryProcessNextPost(IWebDriver driver)
    {
        var postElement = FindElementWithRetries(driver, By.XPath("//img[@data-bloks-name='bk.components.Image']"));
        if (postElement == null)
        {
            "Unable to locate a post that was liked".WriteMessage(MessageType.Warning);
            return false;
        }
        if (!AddToVisitedPosts(postElement)) return true;

        ScrollToElement(driver, postElement);
        OpenAndUnlikePost(driver, postElement);

        return true;
    }

    // 2ac24f8e-fd6a-4f53-bf0f-9d226abe7bfb
    private bool AddToVisitedPosts(IWebElement element)
    {
        try
        {
            //Sometimes the last part of the image source can be the same, so I should use a unique identifier to distinguish them.
            string? srcValue = element.GetDomAttribute("src");
            if (string.IsNullOrWhiteSpace(srcValue)) return false;

            if (!_retryCounts.TryGetValue(srcValue, out int value))
            {
                value = 0;
                _retryCounts[srcValue] = value;
            }

            if (value >= MaxRetryPerPost)
            {
                throw new InvalidOperationException($"Post reached max retry limit ({MaxRetryPerPost}).");
            }

            if (_visitedPosts.Add(srcValue))
            {
                _retryCounts.Remove(srcValue);
                return true;
            }
            else
            {
                _retryCounts[srcValue]++;
                $"Post already visited. Retry #{_retryCounts[srcValue]}.".WriteMessage(MessageType.Warning);
                return false;
            }
        }
        catch (Exception ex)
        {
            $"Error processing post identifier: {ex.Message}".WriteMessage(MessageType.Warning);
            return false;
        }
    }

    private void ScrollToElement(IWebDriver driver, IWebElement element)
    {
        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
        WaitForElementVisible(driver, element);
    }

    private void OpenAndUnlikePost(IWebDriver driver, IWebElement postElement)
    {
        try
        {
            postElement.Click();
            EnsureDomLoaded(driver);

            IWebElement? iconElement = FindElementWithRetries(driver, By.XPath("//*[name()='svg' and @role='img' and (@aria-label='Unlike' or @aria-label='Like') and (contains(@class, 'xyb1xck') or contains(@class, 'xxk16z8'))]"));
            if (iconElement is not null)
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
            }
        }
        catch (Exception ex)
        {
            $"Unexpected error while unliking post #{_unlikedCount}: {ex.Message}".WriteMessage(MessageType.Error);
        }
        finally
        {
            driver.Navigate().Back();
            EnsureDomLoaded(driver);
        }
    }

    private IWebElement? FindElementWithRetries(IWebDriver driver, By by, int retries = MaxRetryPerPost)
    {
        for (int attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                return driver.FindElement(by);
            }
            catch (NoSuchElementException) when (attempt < retries)
            {
                $"Retrying to find element ({attempt}/{retries})...".WriteMessage(MessageType.Warning);
                Task.Delay(2000).Wait();
            }
            catch (Exception ex)
            {
                $"Error finding element: {ex.Message}".WriteMessage(MessageType.Error);
                break;
            }
        }

        return null;
    }

    private void EnsureDomLoaded(IWebDriver driver)
    {
        WebDriverWait wait = new WebDriverWait(driver, AppTimeoutConstants.ExplicitWait);
        wait.Until(d =>
        {
            string? documentReadyState = ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString();
            return documentReadyState is not null && documentReadyState == "complete";
        });
    }

    private void WaitForElementVisible(IWebDriver driver, IWebElement element, int timeoutSeconds = 10)
    {
        new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds))
            .Until(d => element.Displayed);
    }
}