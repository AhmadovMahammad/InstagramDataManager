using DataManager.Constants.Enums;
using DataManager.DesignPatterns.Builder;
using DataManager.DesignPatterns.Strategy;
using DataManager.Helpers.Extensions;
using DataManager.Helpers.Utilities;
using DataManager.Models.JsonModels;
using OpenQA.Selenium;

namespace DataManager.Handlers.ManageHandlers;
public class ManageCloseFriendsHandler() : BaseCommandHandler
{
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
            "No data available to process. Please check the file and try again.".WriteMessage(MessageType.Warning);
            return;
        }

        $"{data.Count()} entries found for processing.".WriteMessage(MessageType.Info);

        if (!ConsoleExtension.AskToProceed("Do you want to remove the listed profiles from close friends? (y/n)"))
        {
            "Operation cancelled by user.".WriteMessage(MessageType.Info);
            return;
        }

        // Build and execute tasks
        var taskBuilder = new SeleniumTaskBuilder(webDriver);
    }
}
