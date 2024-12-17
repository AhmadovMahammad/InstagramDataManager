using ConsoleTables;
using System.Reflection;

namespace DataManager.Extensions;
public static partial class TableExtension
{
    public static void DisplayAsTable<T>(
        this IEnumerable<T> data,
        ConsoleColor consoleColor,
        params string[] columnNames)
    {
        Console.ForegroundColor = consoleColor;
        data.DisplayTable(columnNames);
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
