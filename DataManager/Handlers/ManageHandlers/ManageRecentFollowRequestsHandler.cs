using DataManager.Constants.Enums;
using DataManager.DesignPatterns.Builder;
using DataManager.DesignPatterns.Strategy;
using DataManager.Helpers.Extensions;
using DataManager.Helpers.Utilities;
using DataManager.Models.JsonModels;
using OpenQA.Selenium;

namespace DataManager.Handlers.ManageHandlers;
public class ManageRecentFollowRequestsHandler() : BaseCommandHandler
{
    private const string UnlikeButtonXPath = "";

    public override OperationType OperationType => OperationType.Hybrid;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        string filePath = parameters.Parse<string>("FilePath");
        IFileFormatStrategy strategy = parameters.Parse<IFileFormatStrategy>("FileFormatStrategy");
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");

        ManageData(strategy.ProcessFile(filePath, "relationships_permanent_follow_requests"), webDriver);
    }

    private void ManageData(IEnumerable<RelationshipData> data, IWebDriver webDriver)
    {
        if (!data.Any())
        {
            "No displayable data was found.".WriteMessage(MessageType.Warning);
            return;
        }

        // Display Result
        Console.WriteLine($"Data length: {data.Count()}");
        if (!"\nWould you like to unfollow these pending requests? (y/n)".AskToProceed())
        {
            return;
        }

        // Build and execute
        var taskBuilder = new SeleniumTaskBuilder(webDriver);
    }
}
