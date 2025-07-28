using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 게임의 전체적인 상태와 데이터를 관리
/// </summary>
public class GameManager : Singleton<GameManager>
{
    [Header("게임 진행값")]
    [SerializeField, ReadOnly] private int totalEarnings;
    [SerializeField, ReadOnly] private int currentDay = 1;
    
    [Header("디버그 정보")]
    [SerializeField, ReadOnly] private GamePhase currentPhase;
    [SerializeField] private bool showDebugInfo = true;
    
    private static readonly int[] DaysInMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
    private GamePhase previousPhase;
    
    #region Property
    
    public int TotalEarnings => totalEarnings;
    public int CurrentDay => currentDay;
    public GamePhase CurrentPhase => currentPhase;
    
    #endregion
    
    #region Unity Events
    
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        
        LoadData();
        
        EventBus.OnGameEvent += HandleGameEvent;
        EventBus.OnUIEvent += HandleUIEvent;
    }

    private void OnDestroy()
    {
        EventBus.OnGameEvent -= HandleGameEvent;
        EventBus.OnUIEvent -= HandleUIEvent;
    }
    
    #endregion

    #region 페이즈 및 이벤트 관리

    /// <summary>
    /// 게임 페이즈를 변경, 알림
    /// </summary>
    public void ChangePhase(GamePhase newPhase)
    {
        if (currentPhase == newPhase) return;

        currentPhase = newPhase;
        EventBus.Raise(GameEventType.GamePhaseChanged, newPhase);
        
        Time.timeScale = currentPhase 
            is GamePhase.Paused 
            //or GamePhase.Dialogue
            or GamePhase.GameOver 
            or GamePhase.SelectMenu 
            or GamePhase.Shop ? 0f : 1f;
        
        if (showDebugInfo) Debug.Log($"[GameManager] Game Phase 변경됨: {newPhase}");
    }
    
    public void CheckAndTriggerEditStation()
    {
        if (CurrentPhase == GamePhase.Day && 
            !StoryManager.Instance.HasTriggerableStories(GamePhase.Day))
        {
            ChangePhase(GamePhase.EditStation);
        }
    }
    
    private void HandleGameEvent(GameEventType eventType, object data)
    {
        // RestaurantManager에서 영업 시간 종료 알림 받으면
        if (eventType == GameEventType.RoundTimerEnded)
        {
            ChangePhase(GamePhase.Closing);
        }
    }

    private void HandleUIEvent(UIEventType eventType, object data)
    {
        if (eventType == UIEventType.ShowMenuPanel)
        {
            if (CurrentPhase == GamePhase.EditStation)
                ChangePhase(GamePhase.SelectMenu);
        }
        
        if (eventType == UIEventType.HideResultPanel)
        {
            int earningsFromDay = RestaurantManager.Instance.TodayEarnings;
            EndDayCycle(earningsFromDay);
        }
    }

    /// <summary>
    /// 하루의 모든 사이클이 끝났을 때 호출
    /// </summary>
    private void EndDayCycle(int earningsFromDay)
    {
        IncreaseDay();
        SaveData();
        
        ChangePhase(GamePhase.Day);
        
        if (showDebugInfo) Debug.Log($"[GameManager] 저장 완료. 하루 수입: {earningsFromDay}");
    }
    
    #endregion

    #region 시간 관리
    
    public void PauseGame()
    {
        if (currentPhase == GamePhase.Paused ||
            currentPhase == GamePhase.Dialogue) return;

        // 현재 페이즈 기억
        previousPhase = currentPhase;
        ChangePhase(GamePhase.Paused);
    }
    
    public void ContinueGame()
    {
        if (currentPhase != GamePhase.Paused && 
            currentPhase != GamePhase.Dialogue) return;
        
        ChangePhase(previousPhase);
    }
    
    #endregion
    
    #region 데이터 관리 (돈, 날짜, 저장/불러오기)

    public void AddMoney(int amount)
    {
        totalEarnings += amount;
    }

    private void IncreaseDay()
    {
        currentDay++;
    }
    
    private void LoadData()
    {
        var data = SaveLoadManager.LoadGame();
        if (data == null) return;
        
        totalEarnings = data.totalEarnings;
        currentDay = Mathf.Max(1, data.currentDay);
    }

    private void SaveData()
    {
        var data = SaveLoadManager.LoadGame() ?? new SaveData();
        data.totalEarnings = totalEarnings;
        data.currentDay = currentDay;
        
        SaveLoadManager.SaveGame(data);
        MenuManager.Instance.SaveMenuDatabase();
        
        if (showDebugInfo) Debug.Log("[GameManager]: 게임 데이터 저장 완료.");
    }

    public void GetCurrentDate(out int month, out int day)
    {
        int totalDays = currentDay;
        month = 1;

        foreach (var daysInThisMonth in DaysInMonth)
        {
            if (totalDays > daysInThisMonth)
            {
                totalDays -= daysInThisMonth;
                month++;
            }
            else
            {
                break;
            }
        }
        
        day = totalDays;
    }
    private int backupEarningsBeforeDay = 0;

    public void BackupEarningsBeforeDayStart()
    {
        backupEarningsBeforeDay = totalEarnings;
    }
    public void RestoreEarningsToBeforeDay()
    {
        totalEarnings = backupEarningsBeforeDay;

        // UI 갱신도 함께
        EventBus.Raise(UIEventType.UpdateTotalEarnings, totalEarnings);
    }
    #endregion

    #region Debug Commands
#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(1620, 380, 300, 700));
        
        GUILayout.Label("=== Game Status ===");
        GUILayout.Label($"게임 상태: {Instance.CurrentPhase}");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("모든 메뉴 해금"))
        {
            MenuManager.Instance.UnlockAllMenus();
        }
        
        if (GUILayout.Button("일차 스킵하기"))
        {
            IncreaseDay();
            // TODO: 날짜 변경 호출
        }
        
        if (GUILayout.Button("돈 1000원 추가"))
        {
            AddMoney(1000);
            EventBus.Raise(UIEventType.UpdateTotalEarnings, totalEarnings);
        }
        
        if (GUILayout.Button("돈 1000원 제거"))
        {
            AddMoney(-1000);
            EventBus.Raise(UIEventType.UpdateTotalEarnings, totalEarnings);
        }
        
        GUILayout.Space(20);
        GUILayout.Label("=== Game Phase 변경 ===");

        foreach (GamePhase phase in System.Enum.GetValues(typeof(GamePhase)))
        {
            GUI.enabled = (CurrentPhase != phase);
        
            if (GUILayout.Button(phase.ToString()))
            {
                ChangePhase(phase);
            }
            
            GUI.enabled = true;
        }
        
        GUILayout.EndArea();
    }
#endif    
    #endregion
}

public enum GamePhase
{
    // === DAY SCENE ===
    EditStation, // 배치 편집 ( 배 안에 있을 때 )
    Day,         // 일상 ( 배 밖에 있을 때 )
    SelectMenu, // 메뉴 선택창 켰을 때
    Shop,        // 상점
    
    // === MAIN SCENE ===
    Opening,     // 손님 스폰 전
    Operation,   // 실제 영업중
    Closing,     // 영업 마감
    
    // === 특수 상태 ===
    Dialogue,
    Paused,
    GameOver,
    None,
}