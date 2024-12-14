using DataManager.DesignPatterns.StrategyDP.Contracts;

namespace DataManager.DesignPatterns.StrategyDP.Implementations;
public class JsonFileFormatStrategy : IFileFormatStrategy
{
    public void ProcessFile(string filePath)
    {
        Console.WriteLine("Processing JSON file: " + filePath);
    }
}
