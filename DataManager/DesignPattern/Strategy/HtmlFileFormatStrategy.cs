using DataManager.Constant.Enums;
using DataManager.Helper.Extension;
using DataManager.Models.JsonModels;

namespace DataManager.DesignPattern.Strategy;
public class HtmlFileFormatStrategy : IFileFormatStrategy
{
    public IEnumerable<RelationshipData> ProcessFile(string filePath, string rootElementPath)
    {
        $"Processing HTML file: {filePath}".WriteMessage(MessageType.Info);
        return [];
    }
}
