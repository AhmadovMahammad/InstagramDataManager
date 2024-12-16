﻿using DataManager.DesignPatterns.StrategyDP.Contracts;
using DataManager.Extensions;
using DataManager.Mappers;
using DataManager.Models.Filter;

namespace DataManager.Handlers;
public class DisplayRecentlyUnfollowedHandler() : BaseOperationHandler
{
    public override bool RequiresFile => true;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        string filePath = parameters["FilePath"] as string ?? string.Empty;
        IFileFormatStrategy strategy = parameters["FileFormatStrategy"] as IFileFormatStrategy
                                         ?? throw new ArgumentNullException(nameof(IFileFormatStrategy));

        IEnumerable<RelationshipData> data = strategy.ProcessFile(filePath, "relationships_follow_requests_received");

        Console.WriteLine("\nResult\n");
        RelationshipDataMapper.Map(data).DisplayAsTable(ConsoleColor.Gray, "Title", "Href", "Value", "Timestamp");
    }
}
