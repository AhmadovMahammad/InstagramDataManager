using DataManager.DesignPatterns.StrategyDP.Contracts;
using DataManager.Models.Filter;

namespace DataManager.Handlers;
public class DisplayCloseFriendsHandler() : BaseOperationHandler
{
    public override bool RequiresFile => true;

    public override void Execute(string filePath, IFileFormatStrategy fileFormatStrategy)
    {
        IEnumerable<RelationshipData> data = fileFormatStrategy.ProcessFile(filePath, "relationships_close_friends");
        DisplayResponse(data);
    }
}
