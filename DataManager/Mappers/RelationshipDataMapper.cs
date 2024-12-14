using DataManager.Models.Filter;
using DataManager.Models.View;

namespace DataManager.Mappers;
public class RelationshipDataMapper
{
    public static IEnumerable<DataModel> Map(IEnumerable<RelationshipData> relationshipData)
    {
        return relationshipData.SelectMany(relationship =>
        relationship.StringListData.Select(data => new DataModel(data.Timestamp)
        {
            Title = relationship.Title,
            Href = data.Href,
            Value = data.Value,
        }));
    }
}
