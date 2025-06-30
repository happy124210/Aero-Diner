using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CookingSOGroup", menuName = "CookingGame/CookingSOGroup")]
public class CookingSOGroup : ScriptableObject
{
    public interface IIngredientData
    {
        string GetID(); // 고유 식별자 제공
        string GetDisplayName(); // 이름 (디버그나 UI 출력용)
        Sprite Icon { get; }

    }

    [Serializable]
    public class Entry
    {
        public List<ScriptableObject> rawIngredientList = new(); // 에디터용

        [NonSerialized]
        public List<IIngredientData> ingredients = new(); // 런타임용

        public MenuData menuData;
    }

    [SerializeField]
    public List<Entry> List = new List<Entry>();

    // 빠른 조회를 위한 Dictionary
    private Dictionary<string, MenuData> ingredientMap = new();

    /// <summary>
    /// 내부 Map 다시 구성
    /// </summary>
    private void RebuildMap()
    {
        ingredientMap.Clear();

        foreach (var entry in List)
        {
            entry.ingredients.Clear();

            foreach (var obj in entry.rawIngredientList)
            {
                if (obj is IIngredientData data)
                {
                    entry.ingredients.Add(data);

                    if (!ingredientMap.ContainsKey(data.GetID()))
                    {
                        ingredientMap.Add(data.GetID(), entry.menuData);
                    }
                }
                else
                {
                    Debug.LogWarning($"🔸 '{obj.name}'는 유효한 재료 타입이 아닙니다.");
                }
            }
        }
    }


    public bool Contains(FoodData data)
    {
        return data != null && ingredientMap.ContainsKey(data.id);
    }

    public int GetCount()
    {
        return ingredientMap.Count;
    }

    /// <summary>
    /// 지정한 FoodData와 MenuData를 그룹에 추가. 이미 존재하는 경우 추가하지 않음
    /// </summary>
    public void AddIngredients(List<IIngredientData> ingredientList, MenuData menuData)
    {
        if (ingredientList == null || ingredientList.Count == 0 || menuData == null)
        {
            Debug.LogWarning("재료나 메뉴 데이터가 유효하지 않습니다.");
            return;
        }

        Entry newEntry = new Entry
        {
            ingredients = new List<IIngredientData>(ingredientList),
            menuData = menuData
        };
        List.Add(newEntry);

        foreach (var ingredient in ingredientList)
        {
            if (ingredient != null)
            {
                string id = ingredient.GetID();
                if (!ingredientMap.ContainsKey(id))
                {
                    ingredientMap.Add(id, menuData);
                }
            }
        }

        Debug.Log($"{ingredientList.Count}개 재료 등록됨 → 메뉴: {menuData.menuName}");
    }
}
