using UnityEngine;

[CreateAssetMenu(fileName = "New GameEvent", menuName = "Data/GameEvent Data")]
public class GameEventData : ScriptableObject, IData
{
    public string id { get; set; }
    public int triggerDay;
    public GamePhase triggerPhase;
    public EventType eventType;
    public string parameter; // ex. 대화 ID, 레시피 ID, 아이템 ID
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