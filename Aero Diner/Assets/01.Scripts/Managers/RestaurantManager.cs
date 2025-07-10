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

    [Tooltip("현재까지 경과한 시간")]
    [SerializeField] private float gameTime;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;
    
    [Header("라운드 시간 설정")]
    [Tooltip("하루 제한 시간 (초 단위)")]
    [SerializeField] private float gameTimeLimit;

    private void Update()
    {
        if (gameRunning)
        {
            gameTime += Time.deltaTime;
            
            // 시간 제한 체크
            if (gameTime >= gameTimeLimit)
            {
                EndRestaurant("시간 종료!");
            }
            
            // 목표 달성 체크
            if (customersServed >= targetCustomersServed)
            {
                EndRestaurant("목표 달성!");
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
        GUILayout.Label($"Total Earnings: {GameManager.Instance.TotalEarnings}");
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
                EndRestaurant("수동 정지");
            else
                StartRestaurant();
        }
        
        if (GUILayout.Button("Unlock All Menus"))
        {
            MenuManager.Instance.UnlockAllMenus();
            EventBus.Raise(UIEventType.UpdateMenuPanel);
        }
        
        GUILayout.EndArea();
    }
    
    public void StartRestaurant()
    {
        gameRunning = true;
        gameTime = 0f;
        customersServed = 0;

        GameManager.Instance.LoadEarnings();
        
        if (customerSpawner)
        {
            customerSpawner.StartSpawning();
        }

        // UI 이벤트
        EventBus.Raise(UIEventType.ShowRoundTimer);
        
        if (showDebugInfo) Debug.Log("Restaurant game started!");
    }
    
    public void EndRestaurant(string reason)
    {
        if (gameRunning == false) return;
        
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
        if (showDebugInfo) Debug.Log($"Final Stats - Served: {customersServed}, Earnings: {GameManager.Instance.TotalEarnings}");
        
        //  하루 종료 시점에 저장
        GameManager.Instance.IncreaseDay();
        GameManager.Instance.SaveData();

        if (showDebugInfo) Debug.Log("[RestaurantManager] 저장 완료");

        EventBus.Raise(UIEventType.HideRoundTimer);
        EventBus.Raise(UIEventType.ShowResultPanel);
        EventBus.OnBGMRequested(BGMEventType.PlayResultTheme);
    }

    public void OnCustomerEntered()
    {
        customersVisited++;
    }
    
    // 손님이 결제했을 때 호출되는 메서드
    public void OnCustomerPaid(int amount)
    {
        customersServed++;
        GameManager.Instance.AddMoney(amount);
        
        if (showDebugInfo) Debug.Log($"Customer paid {amount}! Total served: {customersServed}, Total earnings: {GameManager.Instance.TotalEarnings}");
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

    #endregion

    #region Debug Commands

    [ContextMenu("Force End Game")]
    public void ForceEndGame()
    {
        EndRestaurant("강제 종료");
    }
    
    [ContextMenu("Add 10 Customers Served")]
    public void AddCustomersServed()
    {
        customersServed += 10;
        Debug.Log($"Added 10 customers served. Total: {customersServed}");
    }
    
    #endregion
}