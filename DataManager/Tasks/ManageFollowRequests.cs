using DataManager.Constant;
using DataManager.Constant.Enums;
using DataManager.DesignPattern.Builder;
using DataManager.DesignPattern.Strategy;
using DataManager.Helper.Extension;
using DataManager.Helper.Utility;
using DataManager.Models.JsonModels;
using OpenQA.Selenium;

namespace DataManager.Tasks;
public class ManageFollowRequests : BaseTaskHandler
{
    private readonly string _rootElementPath = string.Empty;
    private int _unfollowedCount = 0;
    private int _notAcceptedCount = 0;

    public override OperationType OperationType => OperationType.Hybrid;

    public ManageFollowRequests(CommandType commandType)
    {
        _rootElementPath = commandType switch
        {
            CommandType.Manage_Recent_Follow_Requests => "relationships_permanent_follow_requests",
            CommandType.Manage_Pending_Follow_Requests => "relationships_follow_requests_sent",
            _ => string.Empty
        };
    }

    protected override void Execute(Dictionary<string, object> parameters)
    {
        string filePath = parameters.Parse<string>("FilePath");
        IFileFormatStrategy strategy = parameters.Parse<IFileFormatStrategy>("FileFormatStrategy");
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");

        ManageData(strategy.ProcessFile(filePath, _rootElementPath), webDriver);
    }

    private void ManageData(IEnumerable<RelationshipData> data, IWebDriver webDriver)
    {
        if (!data.Any())
        {
            "No displayable data was found.".WriteMessage(MessageType.Warning);
            return;
        }

        Console.WriteLine($"\n[{DateTime.Now}] Total sent follow requests found in file: {data.Count()}");
        if (!"Would you like to decline these pending requests? (y/n)".AskToProceed())
        {
            Console.WriteLine($"Operation canceled by the user.");
            return;
        }

        // Build and execute
        var taskBuilder = new SeleniumTaskBuilder(webDriver);
        taskBuilder
            .PerformAction((webDriver) => HandleAllPendingRequests(webDriver, data))
            .ExecuteTasks();
    }

    private void HandleAllPendingRequests(IWebDriver webDriver, IEnumerable<RelationshipData> data)
    {
        "\nStarting the process of declining requests...".WriteMessage(MessageType.Info);
        webDriver.EnsureDomLoaded();

        var stringListData = data.SelectMany(relationshipData => relationshipData.StringListData);
        if (stringListData.Any())
        {
            foreach (var childData in stringListData)
            {
                string username = childData.Value;
                Console.WriteLine($"Navigating to profile > {username}");

                if (!TryProcessNextRequest(webDriver, childData.Href))
                {
                    $"Unable to decline the sent request for {username}".WriteMessage(MessageType.Warning);
                    "It seems this request is no longer pending or was not accepted.\n".WriteMessage(MessageType.Warning);
                }
                else
                {
                    $"Successfully unfollowed {username}.".WriteMessage(MessageType.Success);
                    _unfollowedCount++;
                }
            }

            $"All possible requests ({_unfollowedCount}) were handled successfully.".WriteMessage(MessageType.Success);
        }
    }

    private bool TryProcessNextRequest(IWebDriver webDriver, string href)
    {
        try
        {
            webDriver.Navigate().GoToUrl(href);
            webDriver.EnsureDomLoaded();

            IWebElement? requestedButton = webDriver.FindWebElement(By.XPath(XPathConstants.RequestedButton), WebElementPriorityType.Low);

            if (requestedButton != null)
            {
                requestedButton.Click();

                IWebElement? unfollowButton = webDriver.FindWebElement(By.XPath(XPathConstants.UnfollowButton), WebElementPriorityType.Low);
                unfollowButton?.Click();

                return true;
            }

            return false;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
