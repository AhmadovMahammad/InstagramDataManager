using OpenQA.Selenium;

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
            .PerformAction((IWebDriver driver) =>
            {
                Console.WriteLine("Figuring out how many pictures there are...");

                var likedPosts = driver.FindElements(By.XPath("//img[@data-bloks-name='bk.components.Image']"));
                if (likedPosts is not null && likedPosts.Count > 0)
                {
                    Console.WriteLine($"Count: {likedPosts.Count}");
                }
            })
            .PerformAction((IWebDriver driver) => driver.Quit());
    }
}
