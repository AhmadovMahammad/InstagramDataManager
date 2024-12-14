using ConsoleTables;
using DataManager.Helpers.Extensions;
using DataManager.Models;

namespace DataManager.Handlers;
public class BaseHandler
{
    public void DisplayInView(IEnumerable<RelationshipData> relationshipData)
    {
        relationshipData.DisplayAsTable<RelationshipData>(Format.Default, "Title", "Href", "Value", "Timestamp");
    }
}
