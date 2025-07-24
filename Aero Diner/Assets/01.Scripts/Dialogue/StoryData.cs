using UnityEngine;
using System.Collections.Generic;

// 조건 종류
public enum ConditionType
{
    Day,           // N일차 이상/이하/같음
    QuestStatus,   // 특정 퀘스트의 상태
    DialogueEnded, // 특정 대화가 끝났을 때
    Money          // 소지금 이상/이하/같음
}

// 액션의 종류
public enum StoryType
{
    StartDialogue, // 대화 시작
    
    StartQuest,    // 퀘스트 주기
    EndQuest,
    
    UnlockRecipe, // 상점에서 해제
    GiveRecipe,   //
    
    UnlockStation, // 상점에서 해제
    GiveStation,
    
    GiveMoney,     // 골드 주기
    
    LostMoney,     // 골드 뺏기
    GameOver,
    
    None,
}

[System.Serializable]
public class StoryCondition
{
    public ConditionType conditionType;
    public string lValue; // 조건 좌항 (quest_tutorial 등)
    public string @operator; // 연산자 (==, >=, Is 등)
    public string rValue; // 조건 우항 (Completed 등)
    
    // ex. quest_tutorial == Completed
}

[System.Serializable]
public class StoryAction
{
    public StoryType storyType;
    public string targetId;     // 액션의 대상 ID (대화 id 등)
    public string value;        // 액션에 필요한 값 (추후 사용)
}

[CreateAssetMenu(fileName = "StoryData", menuName = "Data/Story Data")]
public class StoryData : ScriptableObject
{
    public string id;
    public GamePhase triggerPhase;
    public List<StoryCondition> conditions; // 발동 조건
    public List<StoryAction> actions; // 순차적으로 실행되는 액션
}