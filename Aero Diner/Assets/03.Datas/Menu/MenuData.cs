using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 메뉴 데이터
/// </summary>
[CreateAssetMenu(fileName = "New Menu Data", menuName = "Game Data/Menu Data")]
public class MenuData : ScriptableObject, CookingSOGroup.IIngredientData
{
    [Header("메뉴 정보")]
    public string id;
    public string menuName;
    public Sprite menuIcon;
    public string description;
    public string[] ingredients; // 필요한 재료들 id List
    public float cookTime;
    public float menuCost;

    public string GetID() => id;
    public string GetDisplayName() => menuName;


    // 예: 필요하면 런타임에 FoodData로 변환하는 로직
    public List<FoodData> ResolveIngredients(List<FoodData> allFoods)
    {
        return allFoods
            .Where(food => ingredients.Contains(food.id))
            .ToList();
    }
}