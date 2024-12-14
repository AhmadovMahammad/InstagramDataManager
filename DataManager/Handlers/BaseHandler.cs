using DataManager.Models;

namespace DataManager.Handlers;
public class BaseHandler
{
    public void DisplayInView(IEnumerable<RelationshipData> relationshipData)
    {
        //var enumerator = relationshipData.GetEnumerator();
        //while (enumerator.MoveNext())
        //{
        //    var x = enumerator.Current;
        //    x.StringListData.ForEach(sld => sld.Href.WriteMessage(MessageType.Info));
        //}
    }
}
