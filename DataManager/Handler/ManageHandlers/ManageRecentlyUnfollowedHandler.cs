using DataManager.Constant.Enums;
using DataManager.DesignPattern.Strategy;
using DataManager.Helper.Extension;
using DataManager.Helper.Utility;
using DataManager.Model.JsonModel;
using DataManager.Models.JsonModels;
using OpenQA.Selenium;

namespace DataManager.Handler.ManageHandlers;

public class ManageRecentlyUnfollowedHandler() : BaseCommandHandler
{
    public override OperationType OperationType => OperationType.Hybrid;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        string filePath = parameters.Parse<string>("FilePath");
        IFileFormatStrategy strategy = parameters.Parse<IFileFormatStrategy>("FileFormatStrategy");
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");

        ManageData(strategy.ProcessFile(filePath, "relationships_unfollowed_users"), webDriver);
    }

    private void ManageData(IEnumerable<RelationshipData> data, IWebDriver webDriver)
    {
        if (!data.Any())
        {
            "No displayable data was found.".WriteMessage(MessageType.Warning);
            return;
        }

        // Display Result
        Console.WriteLine($"[{DateTime.Now}] Total recently unfollowed profiled found: {data.Count()}");
        if (!"\nWould you prefer a table-format display of recently unfollowed followers? (y/n)".AskToProceed())
        {
            Console.WriteLine($"Operation canceled by the user.");
            return;
        }

        IEnumerable<StringListData> stringListData = data.SelectMany(rd => rd.StringListData);
        stringListData.Select(listData =>
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return new
            {
                Username = listData.Value,
                Date = dateTime.AddSeconds(listData.Timestamp).ToLongDateString(),
            };
        }).DisplayAsTable(null, "Username", "Unfollow Date");
    }
}