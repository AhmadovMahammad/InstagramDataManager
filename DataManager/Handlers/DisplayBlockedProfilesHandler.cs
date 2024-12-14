﻿using DataManager.DesignPatterns.StrategyDP.Contracts;
using DataManager.Models.Filter;

namespace DataManager.Handlers;
public class DisplayBlockedProfilesHandler() : BaseHandler, IOperationHandler
{
    public void Execute(string filePath, IFileFormatStrategy fileFormatStrategy)
    {
        IEnumerable<RelationshipData> data = fileFormatStrategy.ProcessFile(filePath, "relationships_blocked_users");
        DisplayResponse(data);
    }
}