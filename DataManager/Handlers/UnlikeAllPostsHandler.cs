using DataManager.Constants;
using DataManager.Constants.Enums;
using DataManager.DesignPatterns.Builder;
using DataManager.Extensions;
using DataManager.Factories;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DataManager.Handlers;
public class UnlikeAllPostsHandler : BaseOperationHandler
{
    private readonly string _operationPath = @"https://www.instagram.com/your_activity/interactions/likes/";
    private int _unlikedCount;
    private readonly HashSet<string> _visitedPosts = [];
    private const int MaxRetries = 3;

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

        if (postElement == null) return false;
        if (!AddToVisitedPosts(postElement)) return true;

        ScrollToElement(driver, postElement);
        OpenAndUnlikePost(driver, postElement);

        return true;
    }

    private bool AddToVisitedPosts(IWebElement element)
    {
        try
        {
            //Sometimes the last part of the image source can be the same, so I should use a unique identifier to distinguish them.
            string? srcValue = element.GetDomAttribute("src");
            if (string.IsNullOrWhiteSpace(srcValue))
            {
                return false;
            }

            string hash = srcValue.GetHashCode().ToString("X");
            return _visitedPosts.Add(hash);
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

            IWebElement? unlikeButton = FindElementWithRetries(driver, By.XPath("//*[name()='svg' and @aria-label='Unlike']"));
            if (unlikeButton is not null)
            {
                unlikeButton.Click();
                $"Successfully unliked post #{++_unlikedCount}.".WriteMessage(MessageType.Success);
            }
            else
            {
                $"Unlike button not found for post #{_unlikedCount}".WriteMessage(MessageType.Warning);
            }
        }
        catch (NoSuchElementException ex)
        {
            $"Element for unliking not found: {ex.Message}".WriteMessage(MessageType.Error);
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

    private IWebElement? FindElementWithRetries(IWebDriver driver, By by, int retries = MaxRetries)
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