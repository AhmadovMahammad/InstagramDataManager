using DataManager.Constants.Enums;
using DataManager.Helpers.Extensions;
using DataManager.Models.JsonModels;

namespace DataManager.DesignPatterns.Strategy;
public class HtmlFileFormatStrategy : IFileFormatStrategy
{
    public IEnumerable<RelationshipData> ProcessFile(string filePath, string rootElementPath)
    {
        $"Processing HTML file: {filePath}".WriteMessage(MessageType.Info);
        return [];
    }
}
