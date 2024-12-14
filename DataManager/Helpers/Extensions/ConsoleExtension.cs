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
        DisplayAsTableWithCustomization(data, columnNames, null);
        Console.ResetColor();
    }

    public static void DisplayAsTableWithCustomization<T>(
        this IEnumerable<T> data,
        string[] columnNames,
        Action<ConsoleTable>? configureTable = null)
    {
        ConsoleTable table = new ConsoleTable(columnNames);

        foreach (var child in data)
        {
            PropertyInfo[] properties = child?.GetType().GetProperties() ?? Array.Empty<PropertyInfo>();
            table.AddRow(properties.Select(prop => prop.GetValue(child)?.ToString()).ToArray());
        }

        // Allow optional customization
        configureTable?.Invoke(table);
        table.Write(Format.Minimal);
    }

    public static void DisplayAsHeader(
        this string header,
        double totalColumns,
        ConsoleColor color = ConsoleColor.White)
    {
        try
        {
            Console.ForegroundColor = color;

            // Create a centered header
            int totalWidth = (int)totalColumns * 20;
            string formattedHeader = header.PadLeft((totalWidth + header.Length) / 2).PadRight(totalWidth);
            Console.WriteLine($"{formattedHeader}\n");
        }
        finally
        {
            Console.ResetColor();
        }
    }
}
