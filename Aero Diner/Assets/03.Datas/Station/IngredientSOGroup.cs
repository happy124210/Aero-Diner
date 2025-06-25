using System;
using UnityEngine;

[CreateAssetMenu(fileName = "IngredientGroup", menuName = "CookingGame/IngredientGroup")]
public class IngredientSOGroup : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public ScriptableObject ingredientData; // 재료 SO
        //public GameObject prefab;               // 연결된 프리팹
    }

    public Entry[] ingredientList;



    internal bool Contains(FoodData currentFoodData)
    {
        throw new NotImplementedException();
    }
}