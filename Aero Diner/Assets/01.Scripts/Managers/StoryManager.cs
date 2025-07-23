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
        if (showDebugInfo) Debug.Log("이벤트 수신됨");
        if (eventType != GameEventType.GamePhaseChanged) return;
        GamePhase currentPhase = (GamePhase)data;

        CheckAndTriggerStories(currentPhase);
    }
    
    private void CheckAndTriggerStories(GamePhase currentPhase)
    {
        // 현재 페이즈와 일치하는 스토리만 필터링
        var storiesToCheck = storyDatabase.Where(story => story.triggerPhase == currentPhase);

        foreach (var story in storiesToCheck)
        {
            if (AreConditionsMet(story.conditions))
            {
                if (showDebugInfo) Debug.Log($"[StoryManager] 스토리 트리거: {story.id} Phase: {currentPhase}");
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
                    // TODO: QuestManager.Instance.GetQuestStatus와 c.R_Value 비교
                    break;
                
                // 대화 끝났는지
                case ConditionType.DialogueEnded:
                    // TODO: 대화 시스템 연동
                    result = true;
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
                    // TODO: QuestManager.Instance.StartQuest 호출
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
    
    #endregion
}
