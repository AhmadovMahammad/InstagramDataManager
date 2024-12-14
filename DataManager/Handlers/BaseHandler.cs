using DataManager.Helpers.Extensions;
using DataManager.Mappers;
using DataManager.Models.Filter;

namespace DataManager.Handlers;
public class BaseHandler
{
    public void DisplayResponse(IEnumerable<RelationshipData> data)
    {
        "Result".DisplayAsHeader(4, ConsoleColor.DarkCyan);
        RelationshipDataMapper.Map(data).DisplayAsTable(ConsoleColor.Gray, "Title", "Href", "Value", "Timestamp");
    }
}
