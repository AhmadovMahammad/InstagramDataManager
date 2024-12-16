using DataManager.DesignPatterns.StrategyDP.Contracts;
using DataManager.Models.Filter;

namespace DataManager.Handlers;
public class DisplayBlockedProfilesHandler() : BaseOperationHandler
{
    public override bool RequiresFile => true;

    public override IEnumerable<RelationshipData> Execute(string filePath, IFileFormatStrategy fileFormatStrategy)
    {
        return fileFormatStrategy.ProcessFile(filePath, "relationships_blocked_users");
    }
}