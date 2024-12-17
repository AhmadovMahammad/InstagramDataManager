namespace DataManager.DesignPatterns.ChainOfResponsibility;
public interface IChainHandler
{
    IChainHandler SetNext(IChainHandler handler);
    bool Handle(string request);
}