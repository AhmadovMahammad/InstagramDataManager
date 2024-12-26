using DataManager.Constants.Enums;
using DataManager.DesignPatterns.Builder;
using DataManager.DesignPatterns.Strategy;
using DataManager.Helpers.Extensions;
using DataManager.Helpers.Utilities;
using DataManager.Models.JsonModels;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DataManager.Handlers.DisplayHandlers;
public class ManageRecentlyUnfollowedHandler() : BaseCommandHandler
{
    public override OperationType OperationType => OperationType.Hybrid;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        string filePath = parameters.Parse<string>("FilePath");
        IFileFormatStrategy strategy = parameters.Parse<IFileFormatStrategy>("FileFormatStrategy");
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");

        //ManageData(strategy.ProcessFile(filePath, "relationships_permanent_follow_requests"));
    }

    private void ManageData(IEnumerable<RelationshipData> data, IWebDriver webDriver)
    {
        if (!data.Any())
        {
            "No displayable data was found.".WriteMessage(MessageType.Warning);
            return;
        }

        Console.WriteLine($"only '{data.Count()}' data was found.");
        if (!ConsoleExtension.AskToProceed("\nWould you like to remove these people from close friends? (y/n)"))
        {
            return;
        }

        // Build and execute 
        var taskBuilder = new SeleniumTaskBuilder(webDriver);
        BuildRemoveAccFromCloseFriendsTask(taskBuilder, data).ExecuteTasks();
    }

    private ITaskBuilder BuildRemoveAccFromCloseFriendsTask(SeleniumTaskBuilder taskBuilder, IEnumerable<RelationshipData> data)
    {
        return taskBuilder.PerformAction((d) =>
        {
            HandleAllCloseFriends(d, data);
        });
    }

    private void HandleAllCloseFriends(IWebDriver webDriver, IEnumerable<RelationshipData> data)
    {
        foreach (var relationship in data)
        {
            StringListData? stringListData = relationship.StringListData.FirstOrDefault();
            if (stringListData != null)
            {
                if (string.IsNullOrWhiteSpace(stringListData.Href))
                {
                    $"Skipping entry with missing Href: {relationship.Title}".WriteMessage(MessageType.Warning);
                    continue;
                }

                try
                {
                    webDriver.Navigate().GoToUrl(stringListData.Href);
                    WebDriverExtension.EnsureDomLoaded(webDriver);

                    IWebElement? unblockButton = FindUnblockButton(webDriver);
                    if (unblockButton != null)
                    {
                        unblockButton.Click();
                        $"Successfully unblocked: {relationship.Title}".WriteMessage(MessageType.Success);
                    }
                    else
                    {
                        $"Unblock button not found for: {relationship.Title}".WriteMessage(MessageType.Warning);
                    }
                }
                catch (Exception ex)
                {
                    $"Error processing {relationship.Title}: {ex.Message}".WriteMessage(MessageType.Error);
                }
            }
        }
    }

    private IWebElement? FindUnblockButton(IWebDriver webDriver)
    {
        return null;
    }
}
