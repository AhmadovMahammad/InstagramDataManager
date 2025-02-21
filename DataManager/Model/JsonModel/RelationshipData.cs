﻿using DataManager.Model.JsonModel;

namespace DataManager.Models.JsonModels;
public class RelationshipData
{
    public string Title { get; set; } = string.Empty;
    public List<MediaListData> MediaListData { get; set; } = [];
    public List<StringListData> StringListData { get; set; } = [];
}
