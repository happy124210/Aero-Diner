using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PassiveGroup", menuName = "CookingGame/PassiveGroup")]
public class PassiveSOGroup : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        // FoodData는 재료 데이터로 ScriptableObject를 상속받았다고 가정합니다.
        public FoodData passiveData;
    }

    public List<Entry> passiveList = new List<Entry>();

    /// <summary>
    /// 그룹 내에 지정한 FoodData가 포함되어있는지 체크
    /// </summary>
    public bool Contains(FoodData data)
    {
        foreach (var entry in passiveList)
        {
            if (entry.passiveData == data)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 그룹에 등록된 요소의 개수를 반환
    /// </summary>
    public int GetCount()
    {
        return passiveList.Count;
    }

    /// <summary>
    /// 이미 등록되어있지 않다면 지정한 FoodData를 그룹에 추가
    /// </summary>
    public void AddIngredient(FoodData data)
    {
        if (!Contains(data))
        {
            Entry newEntry = new Entry();
            newEntry.passiveData = data;
            passiveList.Add(newEntry);
            Debug.Log($"Ingredient '{data.displayName}' added. Total Count: {GetCount()}");
        }
        else
        {
            Debug.Log($"Ingredient '{data.displayName}' is already registered in the group.");
        }
    }
}