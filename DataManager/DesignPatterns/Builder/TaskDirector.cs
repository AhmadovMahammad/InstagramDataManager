using DataManager.Constants.Enums;
using DataManager.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DataManager.DesignPatterns.Builder;
public class TaskDirector
{
    private readonly ITaskBuilder _builder;
    private HashSet<string> _visitedImages;

    public TaskDirector(ITaskBuilder builder)
    {
        _builder = builder;
        _visitedImages = new HashSet<string>();
    }

    public void BuildUnlikeAllPostsTask()
    {
        _builder
            .NavigateTo("https://www.instagram.com/your_activity/interactions/likes/")
            .PerformAction(UnlikeAllVisiblePosts)
            .PerformAction((IWebDriver driver) => driver.Quit());
    }

    private void UnlikeAllVisiblePosts(IWebDriver driver)
    {
        try
        {
            "Starting the process of unliking posts..".WriteMessage(MessageType.Info);

            bool hasMorePosts = true;
            int unlikedPosts = 0;

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Ensure the DOM is ready before starting
            WaitForDomToBeReady(driver);

            while (hasMorePosts)
            {
                IWebElement post = driver.FindElement(By.XPath("//img[@data-bloks-name='bk.components.Image']"));

                if (post == null)
                {
                    "No liked posts found. Exiting process.".WriteMessage(MessageType.Warning);
                    break;
                }

                string postSrc = post.GetDomAttribute("src");
                if (_visitedImages.Contains(postSrc))
                {
                    continue;
                }

                _visitedImages.Add(postSrc);

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", post);
                wait.Until(d => post.Displayed && post.Enabled);

                post.Click();

                WaitForDomToBeReady(driver);

                var unlikeButton = driver.FindElement(By.XPath("//*[name()='svg' and @aria-label='Unlike']"));
                unlikeButton.Click();
                unlikedPosts++;

                $"Unliked post #{unlikedPosts}".WriteMessage(MessageType.Info);

                driver.Navigate().Back();
                WaitForDomToBeReady(driver);

                hasMorePosts = driver.FindElements(By.XPath("//img[@data-bloks-name='bk.components.Image']")).Count > 0;
                if (!hasMorePosts)
                {
                    "No more posts available. Ending process.".WriteMessage(MessageType.Info);
                }
            }

            $"Finished processing all liked posts. Total unliked posts: {unlikedPosts}".WriteMessage(MessageType.Success);
        }
        catch (Exception ex)
        {
            $"An error occurred while processing unlike operation: {ex.Message}".WriteMessage(MessageType.Error);
        }
    }

    private void WaitForDomToBeReady(IWebDriver driver)
    {
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(d =>
        {
            var readyState = ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString();
            return readyState == "complete";
        });
    }
}