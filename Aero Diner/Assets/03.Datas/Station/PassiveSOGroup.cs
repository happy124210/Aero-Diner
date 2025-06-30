using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PassiveGroup", menuName = "CookingGame/PassiveGroup")]
public class PassiveSOGroup : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public FoodData foodData;
    }

    public List<Entry> passiveList = new List<Entry>();

    /// <summary>
    /// 그룹 내에 지정한 FoodData가 포함되어있는지 체크
    /// </summary>
    public bool Contains(FoodData data)
    {
        foreach (var entry in passiveList)
        {
            if (entry.foodData == data)
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
            newEntry.foodData = data;
            passiveList.Add(newEntry);
            Debug.Log($"Ingredient '{data.foodName}' added. Total Count: {GetCount()}");
        }
        else
        {
            Debug.Log($"Ingredient '{data.foodName}' is already registered in the group.");
        }
    }

    public bool ContainsID(string id)
    {
        return passiveList.Any(e => e.foodData != null && e.foodData.id == id);
    }
}