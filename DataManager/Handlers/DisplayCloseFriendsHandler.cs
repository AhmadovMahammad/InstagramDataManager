﻿using DataManager.DesignPatterns.StrategyDP.Contracts;

namespace DataManager.Handlers;
public class DisplayCloseFriendsHandler(string filePath) : IOperationHandler
{
    public void Execute(IFileFormatStrategy fileFormatStrategy)
    {
        throw new NotImplementedException();
    }
}
