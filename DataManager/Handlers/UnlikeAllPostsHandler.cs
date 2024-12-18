using DataManager.DesignPatterns.Builder;
using OpenQA.Selenium;

namespace DataManager.Handlers;
public class UnlikeAllPostsHandler : BaseOperationHandler
{
    private readonly string _operationPath = @"https://www.instagram.com/your_activity/interactions/likes/";
    public override bool RequiresFile => false;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        IWebDriver driver = parameters["WebDriver"] as IWebDriver
                            ?? throw new ArgumentNullException(nameof(IWebDriver));

        ITaskBuilder taskBuilder = new SeleniumTaskBuilder(driver);
        TaskDirector taskDirector = new TaskDirector(taskBuilder);

        taskDirector.BuildUnlikeAllPostsTask();
        taskBuilder.ExecuteTasks();
    }
}
