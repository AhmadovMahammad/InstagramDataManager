using DataManager.Constants.Enums;
using DataManager.DesignPatterns.Builder;
using DataManager.DesignPatterns.Strategy;
using DataManager.Helpers.Extensions;
using DataManager.Helpers.Utilities;
using DataManager.Models.JsonModels;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DataManager.Handlers.DisplayHandlers;
public class ManageFollowingHandler() : BaseCommandHandler
{
    public override OperationType OperationType => OperationType.Hybrid;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        string filePath = parameters.Parse<string>("FilePath");
        IFileFormatStrategy strategy = parameters.Parse<IFileFormatStrategy>("FileFormatStrategy");
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");

        // Process data for relationships if the file contains valid data
        ManageData(strategy.ProcessFile(filePath, "relationships_following"), webDriver);
    }

    private void ManageData(IEnumerable<RelationshipData> data, IWebDriver webDriver)
    {
        if (!data.Any())
        {
            "No displayable data was found.".WriteMessage(MessageType.Warning);
            return;
        }

        $"Found {data.Count()} profiles to process.".WriteMessage(MessageType.Info);

        if (!ConsoleExtension.AskToProceed("\nWould you like to remove these profiles from following? (y/n)"))
        {
            "Operation cancelled by user.".WriteMessage(MessageType.Info);
            return;
        }

        // Build and execute the task
        var taskBuilder = new SeleniumTaskBuilder(webDriver);
        BuildRemoveFromFollowingTask(taskBuilder, data).ExecuteTasks();
    }

    private ITaskBuilder BuildRemoveFromFollowingTask(SeleniumTaskBuilder taskBuilder, IEnumerable<RelationshipData> data)
    {
        foreach (var relationship in data)
        {
            taskBuilder.PerformAction(driver => HandleFollowingProfile(driver, relationship));
        }

        return taskBuilder;
    }

    private void HandleFollowingProfile(IWebDriver webDriver, RelationshipData relationship)
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

            var removeButton = FindRemoveFromFollowingButton(webDriver);
            if (removeButton != null)
            {
                removeButton.Click();
                $"Successfully removed '{relationship.Title}' from following.".WriteMessage(MessageType.Success);
            }
            else
            {
                $"No 'Remove from Following' button found for profile '{relationship.Title}'.".WriteMessage(MessageType.Warning);
            }
        }
        catch (Exception ex)
        {
            $"Error processing '{relationship.Title}': {ex.Message}".WriteMessage(MessageType.Error);
        }
    }

    private IWebElement? FindRemoveFromFollowingButton(IWebDriver webDriver)
    {
        return null;
    }
}
