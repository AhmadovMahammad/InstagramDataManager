﻿using System.Text.Json.Serialization;

namespace DataManager.Models.JsonModels;
public class MediaListData
{
    [JsonPropertyName("media_url")]
    public string MediaUrl { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}