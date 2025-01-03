using DataManager.Constant.Enums;
using DataManager.DesignPattern.Builder;
using DataManager.DesignPattern.Strategy;
using DataManager.Helper.Extension;
using DataManager.Helper.Utility;
using DataManager.Models.JsonModels;
using OpenQA.Selenium;

namespace DataManager.Handler.ManageHandlers;
public class ManageBlockedProfilesHandler() : BaseCommandHandler
{
    private const string UnblockXPath = "//button[contains(@class, '_acan') and contains(@class, '_acap')//div[contains(text(), 'Unblock')]";

    public override OperationType OperationType => OperationType.Hybrid;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        string filePath = parameters.Parse<string>("FilePath");
        IFileFormatStrategy strategy = parameters.Parse<IFileFormatStrategy>("FileFormatStrategy");
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");

        ManageData(strategy.ProcessFile(filePath, "relationships_blocked_users"), webDriver);
    }

    private void ManageData(IEnumerable<RelationshipData> data, IWebDriver webDriver)
    {
        if (!data.Any())
        {
            "No data available to process. Please check the file and try again.".WriteMessage(MessageType.Warning);
            return;
        }

        $"{data.Count()} entries found for processing.".WriteMessage(MessageType.Info);

        if (!"Do you want to unblock the listed profiles? (y/n)".AskToProceed())
        {
            "Operation cancelled by user.".WriteMessage(MessageType.Info);
            return;
        }

        // Build and execute tasks
        var taskBuilder = new SeleniumTaskBuilder(webDriver);
    }
}
