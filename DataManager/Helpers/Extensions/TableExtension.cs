using ConsoleTables;
using System.Reflection;

namespace DataManager.Helpers.Extensions;
public static partial class TableExtension
{
    public static void DisplayAsTable<T>(
        this IEnumerable<T> data,
        Action<ConsoleTable>? configureTable = null,
        params string[] columnNames)
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
