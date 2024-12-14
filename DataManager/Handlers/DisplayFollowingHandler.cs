﻿using DataManager.DesignPatterns.StrategyDP.Contracts;
using DataManager.Models;

namespace DataManager.Handlers;
public class DisplayFollowingHandler() : IOperationHandler
{
    public void Execute(string filePath, IFileFormatStrategy fileFormatStrategy)
    {
        RelationshipData data = fileFormatStrategy.ProcessFile(filePath, "relationships_following");
    }
}
