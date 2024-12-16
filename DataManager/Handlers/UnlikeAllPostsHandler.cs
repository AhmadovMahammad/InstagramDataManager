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

        driver.Navigate().GoToUrl(_operationPath);
        Console.WriteLine("Navigated to the likes interaction page.");
    }
}
