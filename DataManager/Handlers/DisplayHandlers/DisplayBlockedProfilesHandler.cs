using DataManager.DesignPatterns.Strategy;
using DataManager.Extensions;
using DataManager.Mappers;
using DataManager.Models.JsonModels;

namespace DataManager.Handlers.DisplayHandlers;
public class DisplayBlockedProfilesHandler() : BaseOperationHandler
{
    public override bool RequiresFile => true;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        string filePath = parameters["FilePath"] as string ?? string.Empty;
        IFileFormatStrategy strategy = parameters["FileFormatStrategy"] as IFileFormatStrategy
                                         ?? throw new ArgumentNullException(nameof(IFileFormatStrategy));

        IEnumerable<RelationshipData> data = strategy.ProcessFile(filePath, "relationships_blocked_users");

        Console.WriteLine("\nResult\n");
        RelationshipDataMapper.Map(data).DisplayAsTable(ConsoleColor.Gray, "Title", "Href", "Value", "Timestamp");
    }
}