using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DataManager.DesignPatterns.Builder;
public class TaskDirector
{
    private readonly ITaskBuilder _builder;

    public TaskDirector(ITaskBuilder builder)
    {
        _builder = builder;
    }

    public void BuildUnlikeAllPostsTask()
    {
        _builder
            .NavigateTo("https://www.instagram.com/your_activity/interactions/likes/")
            .PerformAction(ScrollToPost)
            .PerformAction(ClickPost)
            .PerformAction(ClickUnlike)
            .PerformAction(GoBackToFeed)
            .PerformAction(CheckForMorePosts)
            .PerformAction((IWebDriver driver) => driver.Quit());
    }

    private void ScrollToPost(IWebDriver driver)
    {
        try
        {
            var post = driver.FindElement(By.XPath("//img[@data-bloks-name='bk.components.Image']"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", post);

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => post.Displayed && post.Enabled);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in scrolling to post: {ex.Message}");
        }
    }

    private void ClickPost(IWebDriver driver)
    {
        try
        {
            var post = driver.FindElement(By.XPath("//img[@data-bloks-name='bk.components.Image']"));
            post.Click();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in clicking post: {ex.Message}");
        }
    }

    private void ClickUnlike(IWebDriver driver)
    {
        try
        {
            var unlikeButton = driver.FindElement(By.XPath("//*[name()='svg' and @aria-label='Unlike']"));
            unlikeButton.Click();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in clicking unlike button: {ex.Message}");
        }
    }

    private void GoBackToFeed(IWebDriver driver)
    {
        try
        {
            driver.Navigate().Back();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in going back to feed: {ex.Message}");
        }
    }

    private void CheckForMorePosts(IWebDriver driver)
    {
        try
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.XPath("//img[@data-bloks-name='bk.components.Image']")));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in checking for more posts: {ex.Message}");
        }
    }
}