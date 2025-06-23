using UnityEngine;

public enum StationType
{
    Shelf,
    Trashcan,
    CuttingBoard,
    FryingPan,
    Pot,
    None, // 빈 공간이면 어디든 상관 없음
}

/// <summary>
/// 음식/재료 데이터
/// </summary>
[CreateAssetMenu(fileName = "New Food Data", menuName = "Game Data/Food Data")]
public class FoodData : ScriptableObject
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
}