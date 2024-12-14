using DataManager.Models;

namespace DataManager.DesignPatterns.StrategyDP.Contracts;
public interface IFileFormatStrategy
{
    IEnumerable<RelationshipData> ProcessFile(string filePath, string rootElementPath);
}
