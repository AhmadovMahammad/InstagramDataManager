using DataManager.Models;

namespace DataManager.DesignPatterns.StrategyDP.Contracts;
public interface IFileFormatStrategy
{
    RelationshipData ProcessFile(string filePath, string rootElementPath);
}
