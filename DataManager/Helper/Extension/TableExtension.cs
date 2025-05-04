using System.Reflection;
using TableTower.Core.Builder;
using TableTower.Core.Models;
using TableTower.Core.Rendering;
using TableTower.Core.Themes;

namespace DataManager.Helper.Extension;
public static partial class TableExtension
{
    public static void DisplayAsTable<T>(
        this IEnumerable<T> data,
        Action<TableOptions>? configureTable = null,
        params string[] columnNames)
    {
        TableBuilder? builder = null;

        if (columnNames.Length == 0)
        {
            builder = new TableBuilder(configureTable)
                .WithData(data)
                .WithTheme(new RoundedTheme());
        }
        else
        {
            builder = new TableBuilder(configureTable)
                .WithColumns(columnNames)
                .WithData(data, true)
                .WithTheme(new RoundedTheme());
        }

        var table = builder.Build();
        new ConsoleRenderer().Print(table);
    }
}
