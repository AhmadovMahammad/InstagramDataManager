using DataManager.DesignPatterns.StrategyDP.Contracts;
using DataManager.Models.Filter;
using System.Text.Json;

namespace DataManager.DesignPatterns.StrategyDP.Implementations;
public class JsonFileFormatStrategy : IFileFormatStrategy
{
    public IEnumerable<RelationshipData> ProcessFile(string filePath, string rootElementPath)
    {
        using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        JsonDocument jsonDocument = JsonDocument.Parse(fileStream, new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
        });

        JsonElement rootElement = jsonDocument.RootElement;
        var data = rootElement.ValueKind switch
        {
            JsonValueKind.Array => ProcessArray(rootElement),
            JsonValueKind.Object => ProcessObject(rootElement, rootElementPath),
            _ => throw new InvalidOperationException($"Unsupported JSON format: {rootElement.ValueKind}. Expected an object or array.")
        };

        return data;
    }

    private IEnumerable<RelationshipData> ProcessArray(JsonElement arrayElement)
    {
        foreach (JsonElement childItem in arrayElement.EnumerateArray())
        {
            if (childItem.ValueKind == JsonValueKind.Object)
            {
                RelationshipData? data = JsonSerializer.Deserialize<RelationshipData>(childItem.GetRawText());
                if (data is not null)
                {
                    yield return data;
                }
            }
        }
    }

    private IEnumerable<RelationshipData> ProcessObject(JsonElement objectElement, string rootElementPath)
    {
        if (string.IsNullOrEmpty(rootElementPath)) throw new ArgumentException("Root element path must be provided for processing JSON objects.", nameof(rootElementPath));
        if (!objectElement.TryGetProperty(rootElementPath, out JsonElement nestedElement)) throw new KeyNotFoundException($"Root element '{rootElementPath}' not found in the JSON.");
        if (nestedElement.ValueKind != JsonValueKind.Array) throw new InvalidOperationException($"The specified root element '{rootElementPath}' is not an array.");

        return ProcessArray(nestedElement);
    }
}
