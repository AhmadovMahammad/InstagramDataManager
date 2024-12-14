using System.Text.Json.Serialization;

namespace DataManager.Models;
public class RelationshipData
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("media_list_data")]
    public List<MediaListData> MediaListData { get; set; } = [];

    [JsonPropertyName("string_list_data")]
    public List<StringListData> StringListData { get; set; } = [];
}
