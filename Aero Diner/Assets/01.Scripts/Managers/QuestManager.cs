using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : Singleton<QuestManager>
{
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo;
    
    private Dictionary<string, QuestData> questDatabase;
    
    // TODO: Save/Load 시스템과 연동
    private Dictionary<string, QuestStatus> playerQuestStatus; // 플레이어의 퀘스트 현황.
    private Dictionary<string, Dictionary<string, int>> playerQuestProgress; // <questId, <targetId, currentAmount>>

    #region 외부 사용 함수
    
    // 진행 중인 모든 퀘스트를 반환
    public List<QuestData> GetInProgressQuests() => GetQuestsByStatus(QuestStatus.InProgress);
    // 완료된 모든 퀘스트를 반환
    public List<QuestData> GetCompletedQuests() => GetQuestsByStatus(QuestStatus.Completed);
    
    // 특정 상태의 모든 퀘스트를 반환
    private List<QuestData> GetQuestsByStatus(QuestStatus status)
    {
        var filteredEntries = playerQuestStatus.Where(pair => pair.Value == status);
        return filteredEntries.Select(pair => questDatabase[pair.Key]).ToList();
    }
    
    public QuestStatus GetQuestStatus(string questId) => playerQuestStatus.GetValueOrDefault(questId, QuestStatus.Inactive);
    
    #endregion
    
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        
        LoadQuestDatabase();
        
        // TODO: 저장에서 playerQuestStatus와 playerQuestProgress 불러오기
        playerQuestStatus = new Dictionary<string, QuestStatus>();
        playerQuestProgress = new Dictionary<string, Dictionary<string, int>>();
    }

    private void LoadQuestDatabase()
    {
        questDatabase = new Dictionary<string, QuestData>();
        QuestData[] allQuests = Resources.LoadAll<QuestData>(StringPath.QUEST_DATA_PATH);
        foreach (var quest in allQuests)
        {
            questDatabase.Add(quest.id, quest);
        }
        if (showDebugInfo) Debug.Log($"[QuestManager] 총 {questDatabase.Count}개 Quest Database 로드 완료");
    }
    
    private QuestData FindQuestByID(string questID) => questDatabase.GetValueOrDefault(questID);
    
    #region 퀘스트 상태 관리
    
    public void StartQuest(string questId)
    {
        if (!questDatabase.ContainsKey(questId))
        {
            if (showDebugInfo) Debug.LogError($"[QuestManager] 존재하지 않는 퀘스트 ID: {questId}");
            return;
        }

        if (playerQuestStatus.ContainsKey(questId))
        {
            if (showDebugInfo) Debug.LogWarning($"[QuestManager] 이미 시작되었거나 완료된 퀘스트: {questId}");
            return;
        }

        if (showDebugInfo) Debug.Log($"[QuestManager] 퀘스트 시작: {questId}");
        playerQuestStatus[questId] = QuestStatus.InProgress;
        playerQuestProgress[questId] = new Dictionary<string, int>();

        // TODO: UI에 퀘스트가 추가되었음을 알리는 이벤트 발생
        // EventBus.Raise(UIEventType.OnQuestStarted, questDatabase[questId]);
    }
    
    public void EndQuest(string questId)
    {
        if (GetQuestStatus(questId) != QuestStatus.InProgress)
        {
            if (showDebugInfo) Debug.LogWarning($"[QuestManager] '{questId}' 퀘스트는 판정할 수 없습니다. 현재 상태: {GetQuestStatus(questId)}");
            return;
        }

        QuestData quest = FindQuestByID(questId);
        if (!quest) return;

        bool allObjectivesMet = true;

        // 퀘스트의 모든 목표를 순회하며 달성 여부 체크
        foreach (var objective in quest.objectives)
        {
            bool isObjectiveMet = false;
            switch (objective.objectiveType)
            {
                // 현재 소지금이 목표 금액 이상인지 확인
                case QuestObjectiveType.EarnMoney:
                    if (GameManager.Instance.TotalEarnings >= objective.requiredAmount)
                    {
                        isObjectiveMet = true;
                    }
                    break;
                
                // 다른 목표 타입에 대한 판정 로직
                // case QuestObjectiveType.BuyStation:
                //     // 
                //     break;
            }
            
            if (!isObjectiveMet)
            {
                allObjectivesMet = false;
                break;
            }
        }
        
        if (allObjectivesMet)
        {
            CompleteQuest(questId);
        }
        else
        {
            FailQuest(questId);
        }
    }


    private bool CompleteQuest(string questId)
    {
        // InProgress인지 확인
        if (GetQuestStatus(questId) != QuestStatus.InProgress)
        {
            if (showDebugInfo) Debug.LogWarning($"[QuestManager] '{questId}' 퀘스트는 해금되지 않음. 현재 상태: {GetQuestStatus(questId)}");
            return false;
        }
        
        QuestData quest = questDatabase[questId];

        // 보상 지급
        if (quest.rewardMoney > 0)
        {
            GameManager.Instance.AddMoney(quest.rewardMoney);
        }
        
        // TODO: 아이템 보상 지급 로직 (StationManager 연동)

        // 퀘스트 상태 변경
        playerQuestStatus[questId] = QuestStatus.Completed;
        if (showDebugInfo) Debug.Log($"[QuestManager] 퀘스트 최종 완료 및 보상 지급: {questId}");

        EventBus.Raise(GameEventType.QuestStatusChanged, new KeyValuePair<string, QuestStatus>(questId, QuestStatus.Completed));
        // TODO: UI 갱신 이벤트
        
        return true;
    }

    private void FailQuest(string questId)
    {
        if (playerQuestStatus.ContainsKey(questId) && playerQuestStatus[questId] == QuestStatus.InProgress)
        {
            playerQuestStatus[questId] = QuestStatus.Failed;
            if (showDebugInfo) Debug.Log($"퀘스트 실패: {questId}");
            EventBus.Raise(GameEventType.QuestStatusChanged, new KeyValuePair<string, QuestStatus>(questId, QuestStatus.Failed));
            // TODO: UI 갱신 이벤트
        }
    }

    #endregion

    #region 퀘스트 진행도 업데이트

    /// <summary>
    /// 퀘스트 목표 달성도 업데이트할 때 사용
    /// </summary>
    public void UpdateQuestProgress(QuestObjectiveType type, string targetId, int amount = 1)
    {
        // 현재 진행 중인 모든 퀘스트를 확인합니다.
        foreach (var questEntry in playerQuestStatus.Where(pair => pair.Value == QuestStatus.InProgress))
        {
            string questId = questEntry.Key;
            QuestData quest = FindQuestByID(questEntry.Key);

            // 해당 퀘스트의 목표들 중에 일치하는 것이 있는지 확인
            foreach (var objective in quest.objectives.Where(obj => obj.objectiveType == type && obj.targetId == targetId))
            {
                if (!playerQuestProgress.ContainsKey(questId)) continue;
                
                // 현재 진행도를 가져와서 업데이트
                int currentAmount = playerQuestProgress[questId].GetValueOrDefault(targetId, 0);
                currentAmount += amount;
                playerQuestProgress[questId][targetId] = currentAmount;
                
                if (showDebugInfo) Debug.Log($"[QuestManager] 퀘스트 진행도 업데이트: {questId} - {targetId} ({currentAmount}/{objective.requiredAmount})");

                // 퀘스트 완료 여부 체크
                CheckQuestCompletion(questId);
            }
        }
    }

    private void CheckQuestCompletion(string questId)
    {
        QuestData quest = questDatabase[questId];
        
        // 모든 목표가 달성되었는지 확인
        foreach (var objective in quest.objectives)
        {
            int currentAmount = playerQuestProgress[questId].GetValueOrDefault(objective.targetId, 0);
            if (currentAmount < objective.requiredAmount)
            {
                return;
            }
        }
        
        // 모든 목표를 달성했다면, 퀘스트 상태를 'Completed'로 변경합니다.
        playerQuestStatus[questId] = QuestStatus.Completed;
        if (showDebugInfo) Debug.Log($"[QuestManager] 퀘스트 목표 달성!: {questId}");
        
        // TODO: UI에 퀘스트를 완료할 수 있다고 알림 (느낌표 등)
    }

    #endregion
}