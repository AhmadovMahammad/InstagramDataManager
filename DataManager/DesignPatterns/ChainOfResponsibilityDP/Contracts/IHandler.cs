namespace DataManager.DesignPatterns.ChainOfResponsibilityDP.Contracts;
public interface IHandler
{
    IHandler SetNext(IHandler handler);
    bool Handle(string request);
}