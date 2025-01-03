using ConsoleTables;
using System.Reflection;

namespace DataManager.Helper.Extension;
public static partial class TableExtension
{
    public static void DisplayAsTable<T>(
        this IEnumerable<T> data,
        Action<ConsoleTable>? configureTable = null,
        params string[] columnNames)
    {
        var table = new ConsoleTable(columnNames);
        bool isSimpleType = typeof(T).IsPrimitive || typeof(T) == typeof(string);

        if (isSimpleType)
        {
            if (columnNames.Length == 2 && data.Count() == 2)
            {
                var countData = data.ToArray();
                table.AddRow(countData.Select(num => num?.ToString()).ToArray());
            }
        }
        else
        {
            foreach (var child in data)
            {
                PropertyInfo[] properties = child?.GetType().GetProperties() ?? Array.Empty<PropertyInfo>();
                table.AddRow(properties.Select(prop => prop.GetValue(child)?.ToString()).ToArray());
            }
        }

        configureTable?.Invoke(table);
        table.Write(Format.Minimal);
    }
}
