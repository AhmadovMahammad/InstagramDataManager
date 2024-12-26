using DataManager.Constants.Enums;
using DataManager.DesignPatterns.Builder;
using DataManager.DesignPatterns.Strategy;
using DataManager.Helpers.Extensions;
using DataManager.Helpers.Utilities;
using DataManager.Models.JsonModels;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DataManager.Handlers.DisplayHandlers;
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

        if (!ConsoleExtension.AskToProceed("Do you want to unblock the listed profiles? (y/n)"))
        {
            "Operation cancelled by user.".WriteMessage(MessageType.Info);
            return;
        }

        // Build and execute tasks
        var taskBuilder = new SeleniumTaskBuilder(webDriver);
        BuildUnblockAccountsTask(taskBuilder, data).ExecuteTasks();
    }

    private ITaskBuilder BuildUnblockAccountsTask(SeleniumTaskBuilder taskBuilder, IEnumerable<RelationshipData> data)
    {
        foreach (var relationship in data)
        {
            taskBuilder.PerformAction(driver => HandleBlockedAccount(driver, relationship));
        }

        return taskBuilder;
    }

    private void HandleBlockedAccount(IWebDriver webDriver, RelationshipData relationship)
    {
        var stringListData = relationship.StringListData.FirstOrDefault(data => !string.IsNullOrWhiteSpace(data?.Href));
        if (stringListData == null)
        {
            $"Skipping profile '{relationship.Title}' due to missing or invalid link.".WriteMessage(MessageType.Warning);
            return;
        }

        try
        {
            webDriver.Navigate().GoToUrl(stringListData.Href);
            WebDriverExtension.EnsureDomLoaded(webDriver);

            var unblockButton = FindUnblockButton(webDriver);
            if (unblockButton != null)
            {
                unblockButton.Click();
                $"Profile '{relationship.Title}' has been successfully unblocked.".WriteMessage(MessageType.Success);
            }
            else
            {
                $"No unblock button found for profile '{relationship.Title}'.".WriteMessage(MessageType.Warning);
            }
        }
        catch (Exception ex)
        {
            $"An error occurred while processing profile '{relationship.Title}': {ex.Message}".WriteMessage(MessageType.Error);
        }
    }

    private IWebElement? FindUnblockButton(IWebDriver webDriver)
    {
        return null;
    }
}
