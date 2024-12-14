namespace DataManager.DesignPatterns.StrategyDP.Contracts;
public interface IFileFormatStrategy
{
    // todo: return one model or something else
    void ProcessFile(string filePath);
}
