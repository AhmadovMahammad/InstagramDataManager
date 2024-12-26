using DataManager.Constants.Enums;
using DataManager.DesignPatterns.Builder;
using DataManager.DesignPatterns.Strategy;
using DataManager.Helpers.Extensions;
using DataManager.Helpers.Utilities;
using DataManager.Models.JsonModels;
using OpenQA.Selenium;

namespace DataManager.Handlers.DisplayHandlers;
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

        if (!ConsoleExtension.AskToProceed("\nWould you like to approve these pending follow requests? (y/n)"))
        {
            "Operation cancelled by user.".WriteMessage(MessageType.Info);
            return;
        }

        // Build and execute the task
        var taskBuilder = new SeleniumTaskBuilder(webDriver);
        BuildApprovePendingFollowRequestsTask(taskBuilder, data).ExecuteTasks();
    }

    private ITaskBuilder BuildApprovePendingFollowRequestsTask(SeleniumTaskBuilder taskBuilder, IEnumerable<RelationshipData> data)
    {
        return taskBuilder.PerformAction((d) =>
        {
            HandlePendingFollowRequest(d, data);
        });
    }

    private void HandlePendingFollowRequest(IWebDriver webDriver, IEnumerable<RelationshipData> data)
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

                    IWebElement? approveButton = FindApproveButton(webDriver);
                    if (approveButton != null)
                    {
                        approveButton.Click();
                        $"Successfully approved follow request for: {relationship.Title}".WriteMessage(MessageType.Success);
                    }
                    else
                    {
                        $"Approve button not found for: {relationship.Title}".WriteMessage(MessageType.Warning);
                    }
                }
                catch (Exception ex)
                {
                    $"Error processing {relationship.Title}: {ex.Message}".WriteMessage(MessageType.Error);
                }
            }
        }
    }

    private IWebElement? FindApproveButton(IWebDriver webDriver)
    {
        return null;
    }
}
