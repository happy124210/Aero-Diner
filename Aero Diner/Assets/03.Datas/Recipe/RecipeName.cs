using UnityEngine;

public enum CookingStation
{
    Shelf,
    Trashcan,
    CuttingBoard,
    FryingPan,
    Pot,
    None, // 빈 공간이면 어디든 상관 없음
}

/// <summary>
/// 레시피 데이터
/// </summary>
[CreateAssetMenu(fileName = "New Recipe Data", menuName = "Game Data/Recipe Data")]
public class RecipeData : ScriptableObject
{
    [Header("레시피 정보")]
    public string id;
    public string recipeName;
    public string[] ingredients; // 필요한 재료들
    public string resId; // 결과 음식 id -> FoodData의 id 참조
    public CookingStation requireStation; // 요구 설비
    public float cookTime;
}