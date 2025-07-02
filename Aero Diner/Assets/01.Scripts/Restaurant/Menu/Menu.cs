using System;
using UnityEngine;

[Serializable]
public class Menu
{
    [Header("기본 데이터")]
    public FoodData foodData;
    
    [Header("플레이어 진행 상황")]
    public bool isUnlocked; // 메뉴가 해금되었는지
    public bool isSelected; // 메뉴가 영업에 사용되는지
    
    [Header("통계")]
    public int cookCount;

    public Menu(FoodData data, bool unlocked = false,  bool selected = false)
    {
        foodData = data;
        isUnlocked = unlocked;
        isSelected = selected;
    }

    public string MenuName => foodData?.displayName ?? "Unknown";
    public int Price => foodData?.foodCost ?? 0;
    public bool CanServeToday => isUnlocked && isSelected;

}
