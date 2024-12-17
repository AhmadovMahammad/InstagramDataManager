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
            .PerformAction(driver =>
            {
                Console.WriteLine("Fetching all liked posts...");
            })
            .PerformAction(driver =>
            {
                Console.WriteLine("Unliking all posts...");
            });
    }
}
