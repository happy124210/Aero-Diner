using UnityEngine;

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
    public Sprite icon;
    public int cost;
    public bool isServe;
}