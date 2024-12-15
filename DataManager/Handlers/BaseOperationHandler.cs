using DataManager.Automation;
using DataManager.Constants.Enums;
using DataManager.DesignPatterns.StrategyDP.Contracts;
using DataManager.Helpers.Extensions;
using DataManager.Mappers;
using DataManager.Models.Filter;

namespace DataManager.Handlers;
public abstract class BaseOperationHandler : IOperationHandler
{
    public abstract bool RequiresFile { get; }

    public void HandleOperation()
    {
        if (RequiresFile)
        {
            FileAutomation fileAutomation = new FileAutomation();
            (string filePath, IFileFormatStrategy? strategy) = fileAutomation.GetParams();

            if (!string.IsNullOrEmpty(filePath) && strategy is not null)
            {
                Execute(filePath, strategy);
            }
        }
        else
        {
            SeleniumAutomation seleniumAutomation = new SeleniumAutomation();
            seleniumAutomation.Execute();
        }
    }

    public virtual void Execute(string filePath, IFileFormatStrategy fileFormatStrategy) { }

    public void DisplayResponse(IEnumerable<RelationshipData> data)
    {
        "\nResult\n".WriteMessage(MessageType.Warning);
        RelationshipDataMapper.Map(data).DisplayAsTable(ConsoleColor.Gray, "Title", "Href", "Value", "Timestamp");
    }
}
