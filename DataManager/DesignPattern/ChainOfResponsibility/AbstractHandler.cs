﻿namespace DataManager.DesignPattern.ChainOfResponsibility;
public abstract class AbstractHandler : IChainHandler
{
    private IChainHandler? _nextHandler;

    public IChainHandler SetNext(IChainHandler handler)
    {
        if (_nextHandler is null)
        {
            _nextHandler = handler;
        }
        else
        {
            IChainHandler currentHandler = _nextHandler;
            while (currentHandler is AbstractHandler abstractHandler && abstractHandler._nextHandler != null)
            {
                currentHandler = abstractHandler._nextHandler;
            }

            (currentHandler as AbstractHandler)!._nextHandler = handler;
        }

        return this;
    }

    public virtual bool Handle(string request) => _nextHandler?.Handle(request) ?? true;
}