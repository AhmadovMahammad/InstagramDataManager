using DataManager.DesignPatterns.ChainOfResponsibilityDP.Contracts;

namespace DataManager.DesignPatterns.ChainOfResponsibilityDP.Implementations;
public abstract class AbstractHandler : IHandler
{
    private IHandler? _nextHandler;

    public IHandler SetNext(IHandler handler)
    {
        if (_nextHandler is null)
        {
            _nextHandler = handler;
        }
        else
        {
            IHandler currentHandler = _nextHandler;
            while (currentHandler is AbstractHandler abstractHandler && abstractHandler._nextHandler is not null)
            {
                currentHandler = abstractHandler._nextHandler;
            }

            (currentHandler as AbstractHandler)!._nextHandler = handler;
        }

        return this;
    }

    public virtual bool Handle(string request) => _nextHandler?.Handle(request) ?? true;
}