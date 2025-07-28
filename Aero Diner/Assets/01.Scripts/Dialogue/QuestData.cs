using UnityEngine;
using System.Collections.Generic;

// 퀘스트의 현재 상태
public enum QuestStatus
{
    Inactive,   // 비활성
    InProgress, // 진행 중
    Completed,  // 완료 (보상 수령까지 끝)
    Failed
}

// 퀘스트 목표의 종류
public enum QuestObjectiveType
{
    EarnMoney,
    
    BuyStation,
    BuyRecipe,
    
    // == 튜토리얼용 ==
    SelectRecipe,
    
    HoldFood,
    HoldStation,
    UseStation,
    DeliverFood,
    
    PlaceStation,
    SelectItemInUI,
}

/// <summary>
/// 퀘스트 개별 목표 정의
/// </summary>
[System.Serializable]
public class QuestObjective
{
    public QuestObjectiveType objectiveType;
    public string targetId;       // 목표 대상 ID
    public int requiredAmount;    // 목표 수량
    // TODO: public int currentAmount; // 현재 달성 수량 (저장/불러오기 필요)
}

[CreateAssetMenu(fileName = "QuestData", menuName = "Data/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("퀘스트 기본 정보")]
    public string id;                //
    public string questName;         //
    public string description;       // 퀘스트 설명
    public string rewardDescription; // 퀘스트 보상 설명
    
    [Header("퀘스트 목표 (AND)")]
    public List<QuestObjective> objectives;

    [Header("퀘스트 보상")]
    public int rewardMoney;
    public string[] rewardItemIds; // 보상 아이템 ID 목록
}