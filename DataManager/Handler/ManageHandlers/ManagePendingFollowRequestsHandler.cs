using ConsoleTables;
using DataManager.Constant.Enums;
using DataManager.DesignPattern.Builder;
using DataManager.DesignPattern.Strategy;
using DataManager.Helper.Extension;
using DataManager.Helper.Utility;
using DataManager.Models.JsonModels;
using OpenQA.Selenium;

namespace DataManager.Handler.ManageHandlers;
public class ManagePendingFollowRequestsHandler() : BaseCommandHandler
{
    private const string PendingButtonXPath = "//button[contains(@class,'_acan') and ..//div[text()='Pending']]";
    private const string UnfollowButtonXPath = "//button[contains(@class, '_a9-- _ap36 _a9-_') and normalize-space(text())='Unfollow']";

    private int _unfollowedCount = 0;
    private int _notUnfollowedCount = 0;

    public override OperationType OperationType => OperationType.Hybrid;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        string filePath = parameters.Parse<string>("FilePath");
        IFileFormatStrategy strategy = parameters.Parse<IFileFormatStrategy>("FileFormatStrategy");
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");

        ManageData(strategy.ProcessFile(filePath, "relationships_follow_requests_sent"), webDriver);
    }

    private void ManageData(IEnumerable<RelationshipData> data, IWebDriver webDriver)
    {
        if (!data.Any())
        {
            "No displayable data was found.".WriteMessage(MessageType.Warning);
            return;
        }

        // Display the total number of pending requests
        Console.WriteLine($"[{DateTime.Now}] Total sent follow requests found: {data.Count()}");
        if (!"\nWould you like to decline or return these pending requests? (y/n)".AskToProceed())
        {
            Console.WriteLine($"Operation canceled by the user.");
            return;
        }

        // Build and execute
        var taskBuilder = new SeleniumTaskBuilder(webDriver);
        taskBuilder
            .PerformAction((IWebDriver webDriver) => HandleAllPendingRequests(webDriver, data))
            .ExecuteTasks();
    }

    private void HandleAllPendingRequests(IWebDriver webDriver, IEnumerable<RelationshipData> data)
    {
        "Starting the process of declining requests...\n".WriteMessage(MessageType.Info);
        WebDriverExtension.EnsureDomLoaded(webDriver);

        var stringListData = data.SelectMany(relationshipData => relationshipData.StringListData);
        if (stringListData.Any())
        {
            foreach (var childData in stringListData)
            {
                Console.WriteLine($"Navigating to profile > {childData.Value}");

                if (!TryProcessNextRequest(webDriver, childData.Href))
                {
                    $"Unable to decline the sent request for {childData.Value}. It seems this request is no longer pending or was not accepted.\n".WriteMessage(MessageType.Warning);
                    _notUnfollowedCount++;
                }
                else
                {
                    _unfollowedCount++;
                }
            }

            "All possible requests were handled successfully.".WriteMessage(MessageType.Success);
            TableExtension.DisplayAsTable([_unfollowedCount, _notUnfollowedCount], (ConsoleTable consoleTable) =>
            {
                consoleTable.Options.EnableCount = false;
            }, "Unfollowed Count", "Declined Count");
        }
    }

    private bool TryProcessNextRequest(IWebDriver webDriver, string href)
    {
        try
        {
            webDriver.Navigate().GoToUrl(href);
            WebDriverExtension.EnsureDomLoaded(webDriver);

            By pendingBy = By.XPath(PendingButtonXPath);
            IWebElement? pendingButton = webDriver.FindElementWithRetries("Requested Button", pendingBy, 1, 1000);

            if (pendingButton == null)
            {
                return false;
            }

            pendingButton.Click();

            By unfollowBy = By.XPath(UnfollowButtonXPath);
            IWebElement? unfollowButton = webDriver.FindElementWithRetries("Unfollow Button", unfollowBy, 2, 1000);

            if (unfollowButton == null)
            {
                $"'Unfollow' button not found on the page.".WriteMessage(MessageType.Error);
                return false;
            }

            unfollowButton.Click();

            $"Successfully unfollowed the request for the user.".WriteMessage(MessageType.Success);
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
