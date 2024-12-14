﻿using DataManager.DesignPatterns.StrategyDP.Contracts;
using DataManager.Models;

namespace DataManager.Handlers;
public class DisplayFollowingHandler() : BaseHandler, IOperationHandler
{
    public void Execute(string filePath, IFileFormatStrategy fileFormatStrategy)
    {
        IEnumerable<RelationshipData> data = fileFormatStrategy.ProcessFile(filePath, "relationships_following");
        DisplayInView(data);
    }
}
