using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레스토랑 게임 매니저
/// </summary>
public class RestaurantManager : Singleton<RestaurantManager>
{
    [Header("Managers")]
    [SerializeField] private CustomerSpawner customerSpawner;
    
    [Header("Layouts")]
    [SerializeField] private Transform entrancePoint;
    [SerializeField] private Transform exitPoint;
    
    [Header("Game State")]
    [SerializeField] private bool gameRunning = true;
    [SerializeField] private int targetCustomersServed;

    [Header("Statistics")]
    [SerializeField] private int customersServed;
    [SerializeField] private int customersVisited;
    [SerializeField] private int totalEarnings;

    [Tooltip("현재까지 경과한 시간")]
    [SerializeField] private float gameTime;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;
    
    [Header("라운드 시간 설정")]
    [Tooltip("하루 제한 시간 (초 단위)")]
    [SerializeField] private float gameTimeLimit;
    [SerializeField] private int currentDay = 1;

    //PlayerPref 저장용 변수
    private const string EARNINGS_KEY = "TotalEarnings";
    private static readonly int[] DaysInMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

    private void Update()
    {
        if (gameRunning)
        {
            gameTime += Time.deltaTime;
            
            // 시간 제한 체크
            if (gameTime >= gameTimeLimit)
            {
                EndGame("시간 종료!");
            }
            
            // 목표 달성 체크
            if (customersServed >= targetCustomersServed)
            {
                EndGame("목표 달성!");
            }
        }
    }
    
    private void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 700));
        
        // 게임 상태 정보
        GUILayout.Label("=== Restaurant Status ===");
        GUILayout.Label($"Game Running: {gameRunning}");
        GUILayout.Label($"Active Customers: {PoolManager.Instance.ActiveCustomerCount}");
        GUILayout.Label($"Customers Served: {customersServed}/{targetCustomersServed}");
        GUILayout.Label($"Total Earnings: {totalEarnings}");
        GUILayout.Label($"Game Time: {gameTime:F1}s / {gameTimeLimit}s");
        
        GUILayout.Space(10);
        
        // 조작 버튼들
        if (GUILayout.Button("Spawn Random Customer"))
        {
            if (customerSpawner)
                customerSpawner.SpawnSingleCustomer();
        }
        
        if (GUILayout.Button(gameRunning ? "Stop Game" : "Start Game"))
        {
            if (gameRunning)
                EndGame("수동 정지");
            else
                StartGame();
        }
        
        if (GUILayout.Button("Unlock All Menus"))
        {
            MenuManager.Instance.UnlockAllMenus();
            EventBus.Raise(UIEventType.UpdateMenuPanel);
        }
        
        GUILayout.EndArea();
    }
    
    public void StartGame()
    {
        gameRunning = true;
        gameTime = 0f;
        customersServed = 0;
        totalEarnings = 0;

        LoadEarnings();
        LoadDay();

        if (customerSpawner)
        {
            customerSpawner.StartSpawning();
        }

        // UI 이벤트
        EventBus.Raise(UIEventType.ShowRoundTimer);
        EventBus.Raise(UIEventType.UpdateEarnings, totalEarnings);
        
        if (showDebugInfo) Debug.Log("Restaurant game started!");
    }
    
    public void EndGame(string reason)
    {
        gameRunning = false;
        
        if (customerSpawner)
        {
            customerSpawner.StopSpawning();
        }

        StartCoroutine(WaitAndCleanup(reason));
    }

    private IEnumerator WaitAndCleanup(string reason)
    {
        if (showDebugInfo) Debug.Log("영업 종료 - 손님들이 떠나기를 기다리는 중...");
        
        TableManager.Instance.ReleaseAllQueues();
        // 모든 손님이 떠날 때까지 대기
        yield return new WaitUntil(() => PoolManager.Instance.ActiveCustomerCount == 0);
        
        if (showDebugInfo) Debug.Log($"Game ended: {reason}");
        if (showDebugInfo) Debug.Log($"Final Stats - Served: {customersServed}, Earnings: {totalEarnings}");
        if (showDebugInfo) Debug.Log($"[RestaurantManager] Incrementing day from {currentDay} → {currentDay + 1}");

        //  하루 증가
        currentDay++;

        //  하루 종료 시점에 저장
        var data = SaveLoadManager.LoadGame() ?? new SaveData();
        data.totalEarnings = totalEarnings;
        data.currentDay = currentDay;
        SaveLoadManager.SaveGame(data);

        if (showDebugInfo) Debug.Log("[RestaurantManager] 저장 완료");

        EventBus.Raise(UIEventType.HideRoundTimer);
        EventBus.Raise(UIEventType.ShowResultPanel);
    }

    public void OnCustomerEntered()
    {
        customersVisited++;
    }
    
    // 손님이 결제했을 때 호출되는 메서드
    public void OnCustomerPaid(int amount)
    {
        customersServed++;
        totalEarnings += amount;

        // UI 이벤트
        EventBus.Raise(UIEventType.UpdateEarnings, totalEarnings);
        
        if (showDebugInfo) Debug.Log($"Customer paid {amount}! Total served: {customersServed}, Total earnings: {totalEarnings}");
    }



    private void LoadEarnings()
    {
        var data = SaveLoadManager.LoadGame();
        totalEarnings = data?.totalEarnings ?? 0;
    }



    private void LoadDay()
    {
        var data = SaveLoadManager.LoadGame();
        currentDay = Mathf.Max(1, data?.currentDay ?? 1);
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
    
    #region public getters

    // 레스토랑 레이아웃
    public Vector3 GetEntrancePoint() => entrancePoint.position;
    public Vector3 GetExitPoint() => exitPoint.position;
    
    // 시간
    public float GameTimeLimit => gameTimeLimit;
    public float CurrentGameTime => gameTime;
    
    // 손님
    public int CustomersServed => customersServed;
    public int CustomersVisited => customersVisited;
    
    // 돈
    public int TotalEarnings => totalEarnings;
    //달력 UI를 위한 날짜 Getter
    public int CurrentDay => currentDay;

    #endregion

    #region Debug Commands

    [ContextMenu("Force End Game")]
    public void ForceEndGame()
    {
        EndGame("강제 종료");
    }
    
    [ContextMenu("Add 10 Customers Served")]
    public void AddCustomersServed()
    {
        customersServed += 10;
        Debug.Log($"Added 10 customers served. Total: {customersServed}");
    }
    
    #endregion
}