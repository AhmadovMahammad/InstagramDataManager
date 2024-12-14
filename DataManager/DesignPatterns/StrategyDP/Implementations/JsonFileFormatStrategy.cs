using DataManager.Constants.Enums;
using DataManager.DesignPatterns.StrategyDP.Contracts;
using DataManager.Helpers.Extensions;
using DataManager.Models;
using System.Text.Json;

namespace DataManager.DesignPatterns.StrategyDP.Implementations;
public class JsonFileFormatStrategy : IFileFormatStrategy
{
    public RelationshipData ProcessFile(string filePath, string rootElementPath)
    {
        $"Processing JSON file: {filePath}".WriteMessage(MessageType.Info);
        Console.WriteLine();

        using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using JsonDocument jsonDocument = JsonDocument.Parse(fileStream, new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
        });

        JsonElement rootElement = jsonDocument.RootElement;

        if (rootElement.TryGetProperty(rootElementPath, out JsonElement jsonElement))
        {

        }

        return new();
    }
}
