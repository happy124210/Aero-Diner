using UnityEngine;

/// <summary>
/// 메뉴 데이터
/// </summary>
[CreateAssetMenu(fileName = "New Menu Data", menuName = "Game Data/Menu Data")]
public class MenuData : ScriptableObject
{
    [Header("메뉴 정보")]
    public string id;
    public string menuName;
    public string[] ingredients; // 필요한 재료들 id List
    public float cookTime;
    public float menuCost;
}