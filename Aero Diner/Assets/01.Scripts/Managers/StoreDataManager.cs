using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoreDataManager : Singleton<StoreDataManager>
{
    public Dictionary<string, StoreItemData> StoreItemMap { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        LoadStoreData(StringPath.STORE_DATA_PATH);
    }

    private void LoadStoreData(string path)
    {
        StoreItemMap = new Dictionary<string, StoreItemData>();
        TextAsset csvFile = Resources.Load<TextAsset>(StringPath.STORE_DATA_CSV_PATH);

        if (csvFile == null)
        {
            Debug.LogError($"CSV 파일 없음: {path}");
            return;
        }

        string[] records = csvFile.text.Split('\n');
        for (int i = 1; i < records.Length; i++)
        {
            string trimmedRecord = records[i].Trim();
            if (string.IsNullOrEmpty(trimmedRecord)) continue;

            string[] fields = trimmedRecord.Split(',');
            if (fields.Length < 5) continue;

            var itemData = new StoreItemData(fields);
            StoreItemMap[itemData.TargetID] = itemData;
        }

        //Debug.Log($"{StoreItemMap.Count}개의 상점 아이템 데이터 로드 완료");
    }
    
    public string GenerateUnlockDescription(StoreItemData itemData)
    {
        switch (itemData.Type)
        {
            case UnlockType.Recipe:
                List<string> requiredRecipeNames = new List<string>();
                foreach (string requiredId in itemData.Conditions)
                {
                    FoodData foodData = MenuManager.Instance.FindMenuById(requiredId).foodData;
                    if (foodData != null)
                    {
                        requiredRecipeNames.Add(foodData.displayName);
                    }
                }
                return $"선행 레시피\n[{string.Join("\nor ", requiredRecipeNames)}] 필요";

            case UnlockType.Quest:
                return StringMessage.QUEST_CONDITION_MESSAGE;

            case UnlockType.None:
                break;

        }

        return null;
    }
}