using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 대사 한 줄에 해당하는 정보 (화자, 표정, 텍스트)
/// </summary>
[System.Serializable]
public struct DialogueLine
{
    public string speakerId;  // SpeakerData의 id
    public string expression; // Happy, Sad 등 초상화용 (나중에 매칭 enum으로 변경할수도)
    public string text;       // 실제 대사 텍스트
}

[System.Serializable]
public struct DialogueChoice
{
    public string text; // UI용 선택지 텍스트
    public string nextDialogueId; // 해당 선택지 후 넘어갈 id
}

/// <summary>
/// CSV의 dialogueData id 하나에 해당하는 전체 대화 묶음 데이터
/// </summary>
[CreateAssetMenu(fileName = "DialogueData", menuName = "Data/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("관리용 ID")]
    public string id;

    [Header("대화 내용")]
    public List<DialogueLine> lines;

    [Header("선택지")]
    public List<DialogueChoice> choices;

    [Header("후속 이벤트")]
    public EventType nextEventType; // 대화 종료 후 발생할 이벤트
    public string nextEventParameter; // 이벤트에 넘겨줄 파라미터 (기본적으로 id)
}

public enum EventType
{
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