﻿using DataManager.DesignPatterns.Strategy;
using DataManager.Extensions;
using DataManager.Mappers;
using DataManager.Models.JsonModels;

namespace DataManager.Handlers.DisplayHandlers;
public class DisplayRecentFollowRequestsHandler() : BaseOperationHandler
{
    public override bool RequiresFile => true;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        string filePath = parameters["FilePath"] as string ?? string.Empty;
        IFileFormatStrategy strategy = parameters["FileFormatStrategy"] as IFileFormatStrategy
                                         ?? throw new ArgumentNullException(nameof(IFileFormatStrategy));

        IEnumerable<RelationshipData> data = strategy.ProcessFile(filePath, "relationships_permanent_follow_requests");

        Console.WriteLine("\nResult\n");
        RelationshipDataMapper.Map(data).DisplayAsTable(null, "Title", "Href", "Value", "Timestamp");
    }
}
