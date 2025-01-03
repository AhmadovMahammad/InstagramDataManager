namespace DataManager.DesignPattern.ChainOfResponsibility;
public interface IChainHandler
{
    IChainHandler SetNext(IChainHandler handler);
    bool Handle(string request);
}