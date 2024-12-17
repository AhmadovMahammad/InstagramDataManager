using DataManager.Models.JsonModels;

namespace DataManager.DesignPatterns.Strategy;
public interface IFileFormatStrategy
{
    IEnumerable<RelationshipData> ProcessFile(string filePath, string rootElementPath);
}
