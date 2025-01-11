using DataManager.Constant.Enums;
using DataManager.DesignPattern.Strategy;
using DataManager.Helper.Extension;
using DataManager.Helper.Utility;
using DataManager.Models.JsonModels;
using OpenQA.Selenium;

namespace DataManager.Tasks;
public class ManageCloseFriends() : BaseTaskHandler
{
    public override OperationType OperationType => OperationType.Hybrid;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        string filePath = parameters.Parse<string>("FilePath");
        IFileFormatStrategy strategy = parameters.Parse<IFileFormatStrategy>("FileFormatStrategy");
        IWebDriver webDriver = parameters.Parse<IWebDriver>("WebDriver");

        ManageData(strategy.ProcessFile(filePath, "relationships_close_friends"), webDriver);
    }

    private void ManageData(IEnumerable<RelationshipData> data, IWebDriver webDriver)
    {
        if (!data.Any())
        {
            "No data available to process. Please check the file and try again.".WriteMessage(MessageType.Warning);
            return;
        }

        $"{data.Count()} entries found for processing.".WriteMessage(MessageType.Info);

        // TODO: complete implementation.
        //if (!"Do you want to remove the listed profiles from close friends? (y/n)".AskToProceed())
        //{
        //    "Operation cancelled by user.".WriteMessage(MessageType.Info);
        //    return;
        //}

        //// Build and execute tasks
        //var taskBuilder = new SeleniumTaskBuilder(webDriver);
    }
}
