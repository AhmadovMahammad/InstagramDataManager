namespace DataManager.DesignPatterns.ChainOfResponsibilityDP.Contracts;
public interface IChainHandler
{
    IChainHandler SetNext(IChainHandler handler);
    bool Handle(string request);
}