using DataManager.Models.JsonModels;

namespace DataManager.DesignPattern.Strategy;
public interface IFileFormatStrategy
{
    IEnumerable<RelationshipData> ProcessFile(string filePath, string rootElementPath);
}
