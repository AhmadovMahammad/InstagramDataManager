using DataManager.DesignPatterns.StrategyDP.Contracts;
using DataManager.Models.Filter;

namespace DataManager.Handlers;
public class DisplayBlockedProfilesHandler() : BaseOperationHandler
{
    public override bool RequiresFile => true;

    public override void Execute(string filePath, IFileFormatStrategy fileFormatStrategy)
    {
        IEnumerable<RelationshipData> data = fileFormatStrategy.ProcessFile(filePath, "relationships_blocked_users");
        DisplayResponse(data);
    }
}