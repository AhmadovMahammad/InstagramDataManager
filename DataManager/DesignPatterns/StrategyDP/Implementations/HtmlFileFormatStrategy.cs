using DataManager.Constants.Enums;
using DataManager.DesignPatterns.StrategyDP.Contracts;
using DataManager.Helpers.Extensions;
using DataManager.Models.Filter;

namespace DataManager.DesignPatterns.StrategyDP.Implementations;
public class HtmlFileFormatStrategy : IFileFormatStrategy
{
    public IEnumerable<RelationshipData> ProcessFile(string filePath, string rootElementPath)
    {
        $"Processing HTML file: {filePath}".WriteMessage(MessageType.Info);
        return [];
    }
}
