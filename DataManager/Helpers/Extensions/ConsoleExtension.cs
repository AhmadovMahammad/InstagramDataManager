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
        ConsoleColor originalColor = Console.ForegroundColor;
        if (MessageTypeColors.TryGetValue(messageType, out var chosenColor))
        {
            Console.ForegroundColor = chosenColor;
        }

        Console.WriteLine(message);
        Console.ForegroundColor = originalColor; // Reset to original color
    }

    public static void DisplayAsTable<T>(this IEnumerable<T> data, Format format = Format.Default, params string[] columnNames)
    {
        ConsoleTable table = new ConsoleTable(columnNames);

        foreach (var child in data)
        {
            PropertyInfo[] properties = child?.GetType().GetProperties() ?? [];
            table.AddRow(properties.Select(prop => prop.GetValue(child)?.ToString()).ToArray());
        }

        table.Write(format);
    }
}
