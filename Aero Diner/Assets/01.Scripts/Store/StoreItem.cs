using System;
using System.Collections.Generic;
using System.Linq;

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