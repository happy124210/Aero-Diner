using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : Singleton<QuestManager>
{
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo;
    
    private Dictionary<string, QuestData> questDatabase;
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
    public QuestData FindQuestByID(string questID) => questDatabase.GetValueOrDefault(questID);
    
    #endregion
    
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        
        LoadQuestDatabase();
        LoadQuestData();
        
        EventBus.OnGameEvent += HandleGameEvent;
    }

    private void OnDestroy()
    {
        EventBus.OnGameEvent -= HandleGameEvent;
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
    
    #region 데이터 저장 및 불러오기

    private void LoadQuestData()
    {
        // 런타임용 Dictionary 초기화
        playerQuestStatus = new Dictionary<string, QuestStatus>();
        playerQuestProgress = new Dictionary<string, Dictionary<string, int>>();

        var data = SaveLoadManager.LoadGame();
        if (data == null)
        {
            if (showDebugInfo) Debug.Log("[QuestManager] 저장 파일 없음. 새로운 퀘스트 데이터로 시작.");
            return;
        }

        // playerQuestStatus 재구성
        if (data.playerQuestStatusKeys != null && data.playerQuestStatusValues != null && data.playerQuestStatusKeys.Count == data.playerQuestStatusValues.Count)
        {
            for (int i = 0; i < data.playerQuestStatusKeys.Count; i++)
            {
                playerQuestStatus[data.playerQuestStatusKeys[i]] = data.playerQuestStatusValues[i];
            }
        }

        // playerQuestProgress 재구성
        if (data.playerQuestProgress != null)
        {
            foreach (var progressData in data.playerQuestProgress)
            {
                var innerDict = new Dictionary<string, int>();
                if (progressData.objectiveTargetIds != null && progressData.objectiveCurrentAmounts != null && progressData.objectiveTargetIds.Count == progressData.objectiveCurrentAmounts.Count)
                {
                    for (int i = 0; i < progressData.objectiveTargetIds.Count; i++)
                    {
                        innerDict[progressData.objectiveTargetIds[i]] = progressData.objectiveCurrentAmounts[i];
                    }
                }
                playerQuestProgress[progressData.questId] = innerDict;
            }
        }
        
        if (showDebugInfo) Debug.Log("[QuestManager] 저장된 퀘스트 데이터 불러오기 및 재구성 완료.");
    }

    public void SaveQuestData()
    {
        var data = SaveLoadManager.LoadGame() ?? new SaveData();

        // playerQuestStatus 변환 및 저장
        data.playerQuestStatusKeys.Clear();
        data.playerQuestStatusValues.Clear();
        foreach (var pair in playerQuestStatus)
        {
            data.playerQuestStatusKeys.Add(pair.Key);
            data.playerQuestStatusValues.Add(pair.Value);
        }

        // playerQuestProgress 변환 및 저장
        data.playerQuestProgress.Clear();
        foreach (var pair in playerQuestProgress)
        {
            var progressData = new SerializableQuestProgress { questId = pair.Key };
            foreach (var innerPair in pair.Value)
            {
                progressData.objectiveTargetIds.Add(innerPair.Key);
                progressData.objectiveCurrentAmounts.Add(innerPair.Value);
            }
            data.playerQuestProgress.Add(progressData);
        }

        SaveLoadManager.SaveGame(data);
        if (showDebugInfo) Debug.Log("[QuestManager] 퀘스트 데이터 변환 및 저장 완료.");
    }

    #endregion
    
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

        // TODO: 퀘스트추가 이벤트 발생
        // EventBus.Raise(UIEventType.OnQuestStarted, questDatabase[questId]);
    }
    
    /// <summary>
    /// 퀘스트 체크하기
    /// </summary>
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
                
                // == 튜토리얼용 퀘스트 ==
                case QuestObjectiveType.HoldFood:
                    if (PlayerController.Instance.IsHoldingFood(objective.targetId))
                    {
                        isObjectiveMet = true;
                    }
                    break;
                
                case QuestObjectiveType.HoldStation:
                    if (PlayerController.Instance.IsHoldingStation(objective.targetId))
                    {
                        isObjectiveMet = true;
                    }
                    break;
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
        if (GetQuestStatus(questId) != QuestStatus.InProgress)
        {
            if (showDebugInfo) Debug.LogWarning($"[QuestManager] '{questId}' 퀘스트는 완료할 수 없는 상태입니다. 현재 상태: {GetQuestStatus(questId)}");
            return false;
        }

        QuestData quest = questDatabase[questId];

        // 보상 지급 로직
        if (quest.rewardMoney > 0)
        {
            GameManager.Instance.AddMoney(quest.rewardMoney);
        }
        
        foreach (var itemId in quest.rewardItemIds)
        {
            if (itemId.StartsWith("s")) StationManager.Instance.UnlockStation(itemId);
            else if (itemId.StartsWith("f")) MenuManager.Instance.UnlockMenu(itemId);
        }
        
        // 퀘스트 ID의 접두사로 최종 상태 결정
        QuestStatus finalStatus;
        if (questId.StartsWith("t_"))
        {
            finalStatus = QuestStatus.Finished; // 튜토리얼 퀘스트 Finished로 처리
        }
        else
        {
            finalStatus = QuestStatus.Completed; // 일반 퀘스트 Completed로 처리
        }

        // 결정된 최종 상태로 퀘스트 상태를 변경
        playerQuestStatus[questId] = finalStatus;

        string statusString = finalStatus == QuestStatus.Finished ? "종료" : "완료";
        if (showDebugInfo) Debug.Log($"[QuestManager] 퀘스트 {statusString} 및 보상 지급: {questId}");

        // 이벤트
        EventBus.Raise(GameEventType.QuestStatusChanged, new KeyValuePair<string, QuestStatus>(questId, finalStatus));
        //EventBus.Raise(UIEventType.UpdateQuestPanel);

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
        List<string> completedQuestIds = new List<string>();
        
        foreach (var questEntry in playerQuestStatus.Where(pair => pair.Value == QuestStatus.InProgress).ToList())
        {
            string questId = questEntry.Key;
            QuestData quest = FindQuestByID(questId);

            // 해당 퀘스트의 목표들 중에 일치하는 것이 있는지 확인
            foreach (var objective in quest.objectives.Where(obj => obj.objectiveType == type && obj.targetId == targetId))
            {
                if (!playerQuestProgress.ContainsKey(questId)) continue;
            
                int currentAmount = playerQuestProgress[questId].GetValueOrDefault(targetId, 0);
                currentAmount += amount;
                playerQuestProgress[questId][targetId] = currentAmount;
            
                if (showDebugInfo) Debug.Log($"[QuestManager] 퀘스트 진행도 업데이트: {questId} - {targetId} ({currentAmount}/{objective.requiredAmount})");

                // 퀘스트 완료 여부 체크 후 완료되었다면 리스트에 추가
                if (CheckQuestCompletion(questId))
                {
                    if (!completedQuestIds.Contains(questId))
                    {
                        completedQuestIds.Add(questId);
                    }
                }
            }
        }

        // 모든 순회가 끝난 후 완료된 퀘스트들의 상태변경
        foreach (var questId in completedQuestIds)
        {
            playerQuestStatus[questId] = QuestStatus.Completed;
            if (showDebugInfo) Debug.Log($"[QuestManager] 퀘스트 목표 달성!: {questId}");
        
            EventBus.Raise(GameEventType.QuestStatusChanged, questId);
        }
    }
    
    /// <summary>
    /// 특정 퀘스트의 특정 목표에 대한 현재 진행도 반환
    /// </summary>
    public int GetQuestObjectiveProgress(string questId, string targetId)
    {
        if (playerQuestProgress.TryGetValue(questId, out var progress))
        {
            if (progress.TryGetValue(targetId, out var currentAmount))
            {
                return currentAmount;
            }
        }

        return 0;
    }

    private bool CheckQuestCompletion(string questId)
    {
        QuestData quest = questDatabase[questId];

        // 모든 목표가 달성되었는지 확인
        foreach (var objective in quest.objectives)
        {
            int currentAmount = playerQuestProgress[questId].GetValueOrDefault(objective.targetId, 0);
            if (currentAmount < objective.requiredAmount)
            {
                return false; // 하나라도 달성 못했으면 false 반환
            }
        }

        return true; // 모든 목표를 달성했으면 true 반환
    }

    #endregion

    #region 이벤트 관리

    private void HandleGameEvent(GameEventType eventType, object payload)
    {
        if (eventType == GameEventType.PlayerPickedUpItem)
        {
            if (payload is FoodData foodData)
            {
                UpdateQuestProgress(QuestObjectiveType.HoldFood, foodData.id);
            }
            
            else if (payload is StationData stationData)
            {
                UpdateQuestProgress(QuestObjectiveType.HoldStation, stationData.id);
            }
        }
    }

    #endregion
}