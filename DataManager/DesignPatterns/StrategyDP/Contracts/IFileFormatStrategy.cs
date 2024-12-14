using DataManager.Models.Filter;

namespace DataManager.DesignPatterns.StrategyDP.Contracts;
public interface IFileFormatStrategy
{
    IEnumerable<RelationshipData> ProcessFile(string filePath, string rootElementPath);
}
