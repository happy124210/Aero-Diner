using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoreItem
{
    // 원본 데이터 참조
    public ScriptableObject BaseData { get; private set; }
    public StoreItemData CsvData { get; private set; }

    // 편의용 속성들
    public string ID => (BaseData is StationData s) ? s.id : ((BaseData is FoodData f) ? f.id : null);
    public string DisplayName => (BaseData is StationData s) ? s.displayName : ((BaseData is FoodData f) ? f.displayName : null);
    public Sprite Icon => (BaseData is StationData s) ? s.stationIcon : ((BaseData is FoodData f) ? f.foodIcon : null);
    public string Description => (BaseData is StationData s) ? s.description : ((BaseData is FoodData f) ? f.description : null);
    public int Cost => CsvData.Cost;

    // 해금 상태 (런타임에 결정됨)
    public bool IsUnlocked { get; set; }
    public bool IsPurchased { get; set; }

    public StoreItem(ScriptableObject baseData, StoreItemData csvData)
    {
        BaseData = baseData;
        CsvData = csvData;
    }
}

[Serializable]
public class StoreItemData
{
    public string ID { get; private set; }
    public string TargetID { get; private set; }
    public UnlockType Type { get; private set; }
    public List<string> Conditions { get; private set; }
    public int Cost { get; private set; }

    public StoreItemData(string[] csvRow)
    {
        ID = csvRow[0];
        TargetID = csvRow[1];
        Enum.TryParse(csvRow[2], true, out UnlockType type);
        Type = type;
        Conditions = csvRow[3].Split('|').ToList();
        int.TryParse(csvRow[4], out int cost);
        Cost = cost;
    }
}

public enum StoreItemType
{
    Station,
    Recipe
}

public enum UnlockType
{
    None,
    Quest,
    Recipe
}