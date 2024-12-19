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
    private int _count;
    private readonly HashSet<string> _visitedPosts = [];

    public override bool RequiresFile => false;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        IWebDriver driver = parameters["WebDriver"] as IWebDriver
                            ?? throw new Exception(nameof(Execute));

        ITaskBuilder builder = new SeleniumTaskBuilder(driver);
        BuildUnlikePostTask(builder).ExecuteTasks();
    }

    private ITaskBuilder BuildUnlikePostTask(ITaskBuilder builder)
    {
        return builder
            .NavigateTo(_operationPath)
            .PerformAction(d => d.Manage().Window.Maximize())
            .PerformAction(ProcessPosts)
            .PerformAction((IWebDriver driver) => driver.Quit());
    }

    private void ProcessPosts(IWebDriver driver)
    {
        "Starting the process of unliking posts..".WriteMessage(MessageType.Info);
        EnsureDomLoaded(driver);

        try
        {
            while (true)
            {
                IWebElement webElement = driver.FindElement(By.XPath("//img[@data-bloks-name='bk.components.Image']"));
                ProcessSinglePost(driver, webElement);

                string currentUrl = driver.Url;
                if (!_operationPath.Contains(currentUrl)) driver.Navigate().GoToUrl(_operationPath);
                else driver.Navigate().Back();

                EnsureDomLoaded(driver);
                Task.Delay(1000).Wait();
            }
        }
        catch (Exception)
        {
            $"An error occurred while processing unlike operation.".WriteMessage(MessageType.Error);
        }
    }

    private void ProcessSinglePost(IWebDriver driver, IWebElement webElement)
    {
        if (TryInsert2History(webElement))
        {
            // scroll to top.
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", webElement);
            new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(_ => webElement.Displayed);

            TryOpenPostAndUnlike(driver, webElement);
        }
    }

    private bool TryInsert2History(IWebElement webElement)
    {
        try
        {
            string srcAttrValue = webElement.GetDomAttribute("src");
            string lastSegment = new Uri(srcAttrValue).Segments.Last().TrimEnd('/');

            return _visitedPosts.Add(lastSegment);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing URL: {ex.Message}");
            return false;
        }
    }

    private void TryOpenPostAndUnlike(IWebDriver driver, IWebElement webElement)
    {
        try
        {
            webElement.Click();
            EnsureDomLoaded(driver);

            Thread.Sleep(1000);

            IWebElement unlikeIcon = driver.FindElement(By.XPath("//*[name()='svg' and @aria-label='Unlike']"));
            unlikeIcon.Click();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SUCCESS] Post #{++_count} unliked successfully.");
        }
        catch (Exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] Unable to unlike post #{++_count}. Moving it on to the following phase.");
        }
        finally
        {
            Console.ResetColor();
        }
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
}
