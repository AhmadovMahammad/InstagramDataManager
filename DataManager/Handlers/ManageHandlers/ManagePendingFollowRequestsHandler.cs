using DataManager.Constants.Enums;
using DataManager.DesignPatterns.Builder;
using DataManager.DesignPatterns.Strategy;
using DataManager.Helpers.Extensions;
using DataManager.Helpers.Utilities;
using DataManager.Models.JsonModels;
using OpenQA.Selenium;

namespace DataManager.Handlers.ManageHandlers;
public class ManagePendingFollowRequestsHandler() : BaseCommandHandler
{
    public override OperationType OperationType => OperationType.Hybrid;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        string filePath = parameters.Parse<string>("FilePath");
        IFileFormatStrategy strategy = parameters.Parse<IFileFormatStrategy>("FileFormatStrategy");
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");

        // Process the file data and manage the pending follow requests
        ManageData(strategy.ProcessFile(filePath, "relationships_pending_follow_requests"), webDriver);
    }

    private void ManageData(IEnumerable<RelationshipData> data, IWebDriver webDriver)
    {
        if (!data.Any())
        {
            "No displayable data was found.".WriteMessage(MessageType.Warning);
            return;
        }

        $"Found {data.Count()} pending follow requests.".WriteMessage(MessageType.Info);

        if (!"\nWould you like to approve these pending follow requests? (y/n)".AskToProceed())
        {
            "Operation cancelled by user.".WriteMessage(MessageType.Info);
            return;
        }

        // Build and execute the task
        var taskBuilder = new SeleniumTaskBuilder(webDriver);
    }
}
