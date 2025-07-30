using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

// 퀘스트의 현재 상태
public enum QuestStatus
{
    Inactive,   // 비활성
    InProgress, // 진행 중
    Completed,  // 완료 (보상 수령까지 끝)
    Failed,
    Finished    // 튜토리얼용 완료 상태 (UI에 표시되지 않음)
}

// 퀘스트 목표의 종류
public enum QuestObjectiveType
{
    EarnMoney,
    
    BuyStation,
    BuyRecipe,
    
    // == 튜토리얼용 ==
    HoldFood,
    DeliverFood,
    
    HoldStation,  // 설비 들고있는지
    UseStation,   // 설비 사용
    PlaceStation, // 설비 배치되었는지
    
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
    public string description;    // 목표 설명
    public string[] requiredIds;    // 필요 대상
}

[CreateAssetMenu(fileName = "QuestData", menuName = "Data/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("퀘스트 기본 정보")]
    public string id;
    public string questName; // UI표시용 이름
    public string description; // 퀘스트 스토리 설명
    public string rewardDescription;

    [Header("퀘스트 목표 (AND)")]
    public List<QuestObjective> objectives;

    [Header("퀘스트 보상")]
    public int rewardMoney;
    public string[] rewardItemIds; // 보상 아이템 ID 목록
}