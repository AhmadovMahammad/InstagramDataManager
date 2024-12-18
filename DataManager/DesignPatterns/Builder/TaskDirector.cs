using DataManager.Constants.Enums;
using DataManager.Extensions;
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

            while (hasMorePosts)
            {
                IWebElement post = driver.FindElement(By.XPath("//img[@data-bloks-name='bk.components.Image']"));

                if (post == null)
                {
                    "No liked posts found. Exiting process.".WriteMessage(MessageType.Warning);
                    break;
                }

                // Scroll the post into view
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", post);

                // Wait until the post is displayed and enabled
                wait.Until(d => post.Displayed && post.Enabled);

                // Click the post
                post.Click();

                // Wait for the post details page to load fully, and check for the unlike button
                var unlikeButton = wait.Until(d => d.FindElement(By.XPath("//*[name()='svg' and @aria-label='Unlike']")));

                // Click the unlike button
                unlikeButton.Click();
                unlikedPosts++;

                $"Unliked post #{unlikedPosts}".WriteMessage(MessageType.Info);

                // Go back to the feed
                driver.Navigate().Back();

                // Wait for the images to load on the main feed after navigating back
                wait.Until(d => d.FindElements(By.XPath("//img[@data-bloks-name='bk.components.Image']")).Count > 0);

                // Check if there are more posts
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
}