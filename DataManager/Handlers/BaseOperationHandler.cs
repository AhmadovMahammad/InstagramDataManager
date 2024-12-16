using DataManager.Automation;
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
                var data = Execute(filePath, strategy);

                Console.WriteLine("\nResult\n");
                RelationshipDataMapper.Map(data).DisplayAsTable(ConsoleColor.Gray, "Title", "Href", "Value", "Timestamp");
            }
        }
        else
        {
            SeleniumAutomation seleniumAutomation = new SeleniumAutomation();
            seleniumAutomation.Execute();
        }
    }

    public virtual IEnumerable<RelationshipData> Execute(string filePath, IFileFormatStrategy fileFormatStrategy)
    {
        return Enumerable.Empty<RelationshipData>();
    }
}
