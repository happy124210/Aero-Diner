using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryManager : Singleton<StoryManager>
{
    [Header("Debug Info")]
    [SerializeField] bool showDebugInfo;
    
    private List<StoryData> storyDatabase;
    private HashSet<string> executedStoryIds = new();
    
    private bool isCurrentSceneUIReady;
    
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        LoadStoryDatabase();
        EventBus.OnGameEvent += HandleGameEvent;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        EventBus.OnGameEvent -= HandleGameEvent;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }
    
    private void OnActiveSceneChanged(Scene current, Scene next)
    {
        isCurrentSceneUIReady = false;
        if (showDebugInfo) Debug.Log($"[StoryManager] 씬 변경: {next.name}. UI 준비 상태 초기화");
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
                if ((GamePhase)data == GamePhase.Paused || (GamePhase)data == GamePhase.Dialogue) return;
                if (showDebugInfo) Debug.Log($"[StoryManager] {(GamePhase)data} Phase 진입");
                CheckAndTriggerStory();
                break;
            
            case GameEventType.UISceneReady:
                isCurrentSceneUIReady = true;
                if (showDebugInfo) Debug.Log("[StoryManager] UISceneReady 이벤트 수신, 스토리 트리거 확인");
                CheckAndTriggerStory();
                break;;
            
            case GameEventType.DialogueEnded:
                string endedDialogueId = data as string;
                if (showDebugInfo) Debug.Log($"[StoryManager] Dialogue '{endedDialogueId}' 종료. 다음 할 일을 확인합니다.");
                CheckAndTriggerStory(endedDialogueId);
                break;
        }
    }
    
    /// <summary>
    /// 실행할 다음 스토리를 찾고, 없으면 다음 단계 결정
    /// </summary>
    /// <param name="endedDialogueId">방금 끝난 대화의 ID (없으면 null)</param>
    private void CheckAndTriggerStory(string endedDialogueId = null)
    {
        if (!isCurrentSceneUIReady)
        {
            if (showDebugInfo) Debug.Log("[StoryManager] UI가 아직 준비되지 않아 스토리 확인 보류");
            
        }
        
        var currentPhase = GameManager.Instance.CurrentPhase;
        
        var nextStory = storyDatabase.FirstOrDefault(story => 
        {
            if (executedStoryIds.Contains(story.id)) return false;

            // 대화가 방금 끝났고, 이 대화를 조건으로 하는 스토리를 찾을 때
            if (endedDialogueId != null && story.triggerPhase == GamePhase.None)
            {
                // 조건 중에 "DialogueEnded"가 있고, 그 ID가 방금 끝난 대화 ID와 일치하는지 확인
                return story.conditions.Any(c => 
                    c.conditionType == ConditionType.DialogueEnded && 
                    c.lValue == endedDialogueId
                );
            }
            
            // 일반적인 단계 변경 시 해당 단계에 맞는 스토리를 찾을 때
            if (endedDialogueId == null && story.triggerPhase == currentPhase)
            {
                return AreConditionsMet(story.conditions);
            }

            return false;
        });

        // 만약 실행할 다음 스토리를 찾았다면
        if (nextStory)
        {
            if (showDebugInfo) Debug.Log($"[StoryManager] 다음 스토리 트리거: {nextStory.id}");
            executedStoryIds.Add(nextStory.id);
            StartCoroutine(ExecuteActions(nextStory.actions));
        }
        
        // 실행할 스토리가 더 이상 없다면
        else if (endedDialogueId == null)
        {
            if (showDebugInfo) Debug.Log($"[StoryManager] {currentPhase} Phase에서 실행할 스토리가 더 이상 없습니다.");
            
            // 현재 단계에 따라 다음 목적지로 이동
            switch (currentPhase)
            {
                case GamePhase.Day:
                    GameManager.Instance.ProceedToEditStation();
                    break;
                case GamePhase.Opening:
                    GameManager.Instance.ProceedToOperation();
                    break;
            }
        }
    }
    
    private IEnumerator ExecuteActions(List<StoryAction> actions)
    {
        foreach (var action in actions)
        {
            switch (action.storyType)
            {
                case StoryType.StartDialogue:
                    DialogueManager.Instance.StartDialogue(action.targetId);
                    break;
                case StoryType.StartQuest:
                    QuestManager.Instance.StartQuest(action.targetId);
                    break;
                case StoryType.EndQuest:
                    QuestManager.Instance.EndQuest(action.targetId);
                    break;
                case StoryType.GiveMoney:
                    GameManager.Instance.AddMoney(int.Parse(action.targetId));
                    break;
                case StoryType.LostMoney:
                    GameManager.Instance.AddMoney(-int.Parse(action.targetId));
                    break;
                
                // 튜토리얼용
                case StoryType.ShowGuideUI:
                    UIEventCaller.CallUIEvent(action.targetId);
                    break;
                case StoryType.ForceUI:
                    //TODO: 특정 패널 강제로 열기 (상점, 퀘스트패널 등)
                    //UIManager.Instance.ShowUI(action.targetId);
                    break;
                case StoryType.ActivateStation:
                    //TODO: 특정 설비 활성화
                    //StationManager.Instance.ActivateStation(action.targetId);
                    break;
                case StoryType.SetTutorialMode:
                    GameManager.Instance.SetTutorialMode(true);
                    break;
                case StoryType.SpawnCustomer:
                    RestaurantManager.Instance.SpawnTutorialCustomer();
                    break;

            }
            yield return null;
        }
    }

    private bool AreConditionsMet(List<StoryCondition> conditions)
    {
        if (conditions == null || conditions.Count == 0) return true;
        foreach (var c in conditions)
        {
            bool result = false;
            switch (c.conditionType)
            {
                case ConditionType.Day:
                    result = CheckNumericCondition(GameManager.Instance.CurrentDay, c.@operator, c.rValue);
                    break;
                case ConditionType.QuestStatus:
                    result = CheckQuestStatusCondition(c.lValue, c.@operator, c.rValue);
                    break;
                case ConditionType.DialogueEnded:
                    result = true; 
                    break;
            }
            if (!result) return false;
        }
        return true;
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