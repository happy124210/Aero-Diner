using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoryManager : Singleton<StoryManager>
{
    [Header("Debug Info")]
    [SerializeField] bool showDebugInfo;
    
    private List<StoryData> storyDatabase;
    private string endedDialogueId = null;
    private HashSet<string> executedStoryIds = new();
    
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        LoadStoryDatabase();
    }

    private void Start()
    {
        EventBus.OnGameEvent += HandleGameEvent;
    }

    private void OnDestroy()
    {
        EventBus.OnGameEvent -= HandleGameEvent;
    }

    private void LoadStoryDatabase()
    {
        storyDatabase = Resources.LoadAll<StoryData>(StringPath.STORY_DATA_PATH).ToList();
    }
    
    private void HandleGameEvent(GameEventType eventType, object data)
    {
        switch (eventType)
        {
            case GameEventType.GamePhaseChanged:
            {
                GamePhase currentPhase = (GamePhase)data;
                CheckAndTriggerStories(triggerPhase: currentPhase);
                break;
            }
            
            case GameEventType.DialogueEnded:
                endedDialogueId = data as string;
                CheckAndTriggerStories(endedDialogue: endedDialogueId);
                endedDialogueId = null;
                break;
        }
    }
    
    private void CheckAndTriggerStories(GamePhase? triggerPhase = null, string endedDialogue = null)
    {
        foreach (var story in storyDatabase)
        {
            if (executedStoryIds.Contains(story.id))
            {
                continue;
            }
            
            bool isCorrectTrigger = triggerPhase.HasValue && story.triggerPhase == triggerPhase.Value 
                                    || endedDialogue != null && story.conditions.Any(c => c.conditionType == ConditionType.DialogueEnded);

            if (isCorrectTrigger && AreConditionsMet(story.conditions))
            {
                if (showDebugInfo) Debug.Log($"[StoryManager] 스토리 트리거: {story.id}");
                executedStoryIds.Add(story.id); 
                StartCoroutine(ExecuteActions(story.actions));
                break;
            }
        }
    }
    
    /// <summary>
    /// 조건 체크 
    /// </summary>
    private bool AreConditionsMet(List<StoryCondition> conditions)
    {
        if (conditions == null || conditions.Count == 0) return true;
        
        foreach (var c in conditions)
        {
            bool result = false;
            
            switch (c.conditionType)
            {
                // 날짜 체크
                case ConditionType.Day:
                    result = CheckNumericCondition(GameManager.Instance.CurrentDay, c.@operator, c.rValue);
                    break;
                
                // 퀘스트 상태 체크
                case ConditionType.QuestStatus:
                    result = CheckQuestStatusCondition(c.lValue, c.@operator, c.rValue);
                    break;
                
                // 대화 끝났는지
                case ConditionType.DialogueEnded:
                    if (c.@operator == "==")
                    {
                        result = (c.lValue == endedDialogueId);
                    }
                    break;
                
                case ConditionType.Money:
                    result = CheckNumericCondition(GameManager.Instance.TotalEarnings, c.@operator, c.rValue);
                    break;
            }
            
            if (!result) return false;
        }
        return true;
    }

    private IEnumerator ExecuteActions(List<StoryAction> actions)
    {
        foreach (var action in actions)
        {
            switch (action.storyType)
            {
                case StoryType.StartDialogue:
                    DialogueManager.Instance.StartDialogue(action.targetId);
                    yield return new WaitUntil(() => GameManager.Instance.CurrentPhase != GamePhase.Dialogue);
                    break;
                
                case StoryType.StartQuest:
                    QuestManager.Instance.StartQuest(action.targetId);
                    break;
                
                case StoryType.LostMoney:
                    GameManager.Instance.AddMoney(int.Parse(action.value));
                    break;
                
                case StoryType.EndQuest:
                    QuestManager.Instance.EndQuest(action.targetId);
                    break;
                    
                case StoryType.GameOver:
                    // 시작 화면으로 돌아가기
                    break;
            }

            yield return null;
        }
    }
    
    #region helper
    
    /// <summary>
    /// 숫자 값을 연산자 기반으로 비교하는 헬퍼 메서드
    /// </summary>
    /// <param name="currentValue"> 현재 게임 값 </param>
    /// <param name="op"> 비교 연산자 (예: "==", ">=") </param>
    /// <param name="requiredValueStr"> 비교할 목표 값 </param>
    /// <returns> 비교 결과가 참이면 true </returns>
    private bool CheckNumericCondition(int currentValue, string op, string requiredValueStr)
    {
        // CSV의 문자열 값을 숫자로 변환
        if (!int.TryParse(requiredValueStr, out int requiredValue))
        {
            Debug.LogWarning($"[StoryManager] 숫자 값 비교 실패: '{requiredValueStr}' 유효하지 않음");
            return false;
        }

        // 비교
        switch (op)
        {
            case "==": return currentValue == requiredValue;
            case "!=": return currentValue != requiredValue;
            case ">":  return currentValue > requiredValue;
            case ">=": return currentValue >= requiredValue;
            case "<":  return currentValue < requiredValue;
            case "<=": return currentValue <= requiredValue;
            default:
                Debug.LogWarning($"[StoryManager] 알 수 없는 연산자: '{op}'");
                return false;
        }
    }
    
    /// <summary>
    /// 퀘스트 상태를 연산자 기반으로 비교하는 헬퍼 메서드
    /// </summary>
    private bool CheckQuestStatusCondition(string questId, string op, string requiredStatusStr)
    {
        QuestStatus currentStatus = QuestManager.Instance.GetQuestStatus(questId);
        
        switch (op)
        {
            case "==":
                return currentStatus.ToString().Equals(requiredStatusStr, StringComparison.OrdinalIgnoreCase);
            case "!=":
                return !currentStatus.ToString().Equals(requiredStatusStr, StringComparison.OrdinalIgnoreCase);
            default:
                Debug.LogWarning($"[StoryManager] 퀘스트 상태에 사용할 수 없는 연산자: '{op}'");
                return false;
        }
    }
    
    #endregion
}
