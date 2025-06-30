using UnityEngine;

/// <summary>
/// 음식/재료 데이터
/// </summary>
[CreateAssetMenu(fileName = "New Food Data", menuName = "Game Data/Food Data")]
public class FoodData : ScriptableObject, CookingSOGroup.IIngredientData
{
    [Header("음식 정보")]
    public string id;
    public string foodName;
    public string displayName;
    public Sprite foodIcon;
    public Sprite processedIcon;
    public string description;
    public StationType requireStation;
    public int foodCost;

    public string GetID() => id;
    public string GetDisplayName() => foodName;

    public override bool Equals(object obj)
    {
        if (obj is FoodData other)
            return this.foodName == other.foodName; // 필요 시 더 많은 필드 비교
        return false;
    }

    public override int GetHashCode()
    {
        return foodName.GetHashCode();
    }
}