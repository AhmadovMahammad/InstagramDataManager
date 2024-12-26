﻿using DataManager.Constants.Enums;
using DataManager.DesignPatterns.Builder;
using DataManager.DesignPatterns.Strategy;
using DataManager.Helpers.Extensions;
using DataManager.Helpers.Utilities;
using DataManager.Models.JsonModels;
using OpenQA.Selenium;

namespace DataManager.Handlers.DisplayHandlers;
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
        if (!ConsoleExtension.AskToProceed("\nWould you like to unfollow these pending requests? (y/n)"))
        {
            return;
        }

        // Build and execute
        var taskBuilder = new SeleniumTaskBuilder(webDriver);
        BuildUnfollowTasks(taskBuilder, data).ExecuteTasks();
    }

    private ITaskBuilder BuildUnfollowTasks(ITaskBuilder taskBuilder, IEnumerable<RelationshipData> data)
    {
        return taskBuilder.PerformAction((d) => HandleAllPendingRequests(d, data));
    }

    private void HandleAllPendingRequests(IWebDriver webDriver, IEnumerable<RelationshipData> data)
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

                    IWebElement? unfollowButton = FindUnfollowButton(webDriver);
                    if (unfollowButton != null)
                    {
                        unfollowButton.Click();
                        "Successfully unfollowed: {relationship.Title}".WriteMessage(MessageType.Success);
                    }
                    else
                    {
                        $"Unfollow button not found for: {relationship.Title}".WriteMessage(MessageType.Warning);
                    }
                }
                catch (Exception ex)
                {
                    $"Error processing {relationship.Title}: {ex.Message}".WriteMessage(MessageType.Error);
                }
            }
        }
    }

    private IWebElement? FindUnfollowButton(IWebDriver webDriver)
    {
        return null;
    }
}
