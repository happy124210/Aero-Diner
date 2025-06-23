using System;
using UnityEngine;

[CreateAssetMenu(fileName = "IngredientGroup", menuName = "CookingGame/IngredientGroup")]
public class IngredientSOGroup : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public ScriptableObject ingredientData; // 재료 SO (예: TomatoSO.asset)
        public GameObject prefab;               // 연결된 프리팹
    }

    public Entry[] ingredientList;

    public GameObject GetPrefabByData(ScriptableObject target)
    {
        foreach (var entry in ingredientList)
        {
            if (entry.ingredientData == target)
                return entry.prefab;
        }
        return null;
    }

    internal bool Contains(FoodData currentFoodData)
    {
        throw new NotImplementedException();
    }
}