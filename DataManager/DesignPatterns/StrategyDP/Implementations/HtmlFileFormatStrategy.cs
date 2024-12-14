using DataManager.DesignPatterns.StrategyDP.Contracts;

namespace DataManager.DesignPatterns.StrategyDP.Implementations;
public class HtmlFileFormatStrategy : IFileFormatStrategy
{
    public void ProcessFile(string filePath)
    {
        Console.WriteLine("Processing HTML file: " + filePath);
    }
}
