using System.Collections.Generic;
using UnityEngine;

public enum StationType
{
    // 조리기구 (수동형 + 자동형 공통)
    CuttingBoard,    // 도마
    FryingPan,       // 프라이팬
    Pot,             // 냄비
    Grater,
    Grinding,
    Mixing,
    //Oven,            // 오븐
    //Blender,         // 믹서기
    //MixingBowl,      // 믹싱 볼


    // 기타 스테이션
    IngredientBox,   // 재료 박스
    Fridge,          // 냉장고 (재료)
    Shelf,           // 선반
    Trashcan,        // 쓰레기통
    None             // 빈 공간
}

public enum WorkType
{
    None,
    Automatic,
    Passive
}

[CreateAssetMenu(fileName = "StationData", menuName = "CookingGame/StationData")]
public class StationData : ScriptableObject
{
    [Header("설비 정보")]
    public string id;
    public string stationName;
    public StationType stationType;
    public WorkType workType ;
    public Sprite stationSprite;
    public Sprite stationIcon;
    public string description;
    public int stationCost;

    // 해당 스테이션에서 지원하는 레시피 목록
    public List<MenuData> availableRecipes;
}
