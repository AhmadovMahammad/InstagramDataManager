using ConsoleTables;
using DataManager.Constants.Enums;
using System.Reflection;

namespace DataManager.Helpers.Extensions;
public static class ConsoleExtension
{
    private static readonly Dictionary<MessageType, ConsoleColor> MessageTypeColors = new()
        {
            { MessageType.Success, ConsoleColor.Green },
            { MessageType.Error, ConsoleColor.DarkRed },
            { MessageType.Info, ConsoleColor.Blue },
            { MessageType.Warning, ConsoleColor.Yellow }
        };

    public static void WriteMessage(this string message, MessageType messageType)
    {
        if (MessageTypeColors.TryGetValue(messageType, out var chosenColor))
        {
            Console.ForegroundColor = chosenColor;
        }

        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void DisplayAsTable<T>(
        this IEnumerable<T> data,
        ConsoleColor consoleColor,
        params string[] columnNames)
    {
        Console.ForegroundColor = consoleColor;
        DisplayTable(data, columnNames);
        Console.ResetColor();
    }

    public static void DisplayTable<T>(
        this IEnumerable<T> data,
        string[] columnNames,
        Action<ConsoleTable>? configureTable = null)
    {
        var table = new ConsoleTable(columnNames);

        foreach (var child in data)
        {
            PropertyInfo[] properties = child?.GetType().GetProperties() ?? Array.Empty<PropertyInfo>();
            table.AddRow(properties.Select(prop => prop.GetValue(child)?.ToString()).ToArray());
        }

        configureTable?.Invoke(table);
        table.Write(Format.Minimal);
    }
}