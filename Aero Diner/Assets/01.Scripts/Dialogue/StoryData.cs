using UnityEngine;
using System.Collections.Generic;

// 조건 종류
public enum ConditionType
{
    Day,           // N일차 이상/이하/같음
    QuestStatus,   // 특정 퀘스트의 상태
    DialogueEnded, // 특정 대화가 끝났을 때
}

// 액션의 종류
public enum StoryType
{
    Tutorial,
    StartDialogue,
    
    StartQuest,
    EndQuest,
    
    UnlockRecipe,
    UnlockStation,
    
    GiveMoney,
    GiveRecipe,
    GiveStation,
    
    LostMoney,
    GameOver,
    
    None,
}

[System.Serializable]
public class StoryCondition
{
    public ConditionType conditionType;
    public string lValue; // 조건 좌항 (quest_frying_pan)
    public string @operator; // 연산자 (==, >=, Is)
    public string rValue; // 조건 우항 (Completed)
}

[System.Serializable]
public class StoryAction
{
    public StoryType storyType;
    public string targetId;     // 액션의 대상 ID (대화 id 등)
}

[CreateAssetMenu(fileName = "StoryData", menuName = "Data/Story Data")]
public class StoryData : ScriptableObject
{
    public string id;
    public List<StoryCondition> conditions; // 발동 조건
    public List<StoryAction> actions; // 순차적으로 실행되는 액션
}