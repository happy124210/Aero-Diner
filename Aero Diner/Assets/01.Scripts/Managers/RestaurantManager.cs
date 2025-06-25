using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레스토랑 게임 매니저 - 전체 시스템 제어
/// </summary>
public class RestaurantGameManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private CustomerSpawner customerSpawner;
    
    [Header("Game State")]
    [SerializeField] private bool gameRunning = true;
    [SerializeField] private int targetCustomersServed = 50;
    [SerializeField] private float gameTimeLimit = 300f; // 5분
    
    [Header("Statistics")]
    [SerializeField] private int customersServed = 0;
    [SerializeField] private int totalEarnings = 0;
    [SerializeField] private float gameTime = 0f;
    
    private void Start()
    {
        StartGame();
    }
    
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
    
    public void StartGame()
    {
        gameRunning = true;
        gameTime = 0f;
        customersServed = 0;
        totalEarnings = 0;
        
        if (customerSpawner != null)
        {
            customerSpawner.StartSpawning();
        }
        
        Debug.Log("Restaurant game started!");
    }
    
    public void EndGame(string reason)
    {
        gameRunning = false;
        
        if (customerSpawner != null)
        {
            customerSpawner.StopSpawning();
        }
        
        Debug.Log($"Game ended: {reason}");
        Debug.Log($"Final Stats - Served: {customersServed}, Earnings: {totalEarnings}, Time: {gameTime:F1}s");
    }
    
    public void RestartGame()
    {
        // 모든 손님 정리 (좌석도 자동으로 해제됨)
        if (customerSpawner != null)
        {
            customerSpawner.ClearAllCustomers();
        }
        
        // 게임 재시작
        StartGame();
        
        Debug.Log("Restaurant game restarted!");
    }
    
    // 손님이 결제했을 때 호출되는 메서드
    public void OnCustomerPaid(int amount)
    {
        customersServed++;
        totalEarnings += amount;
        
        Debug.Log($"Customer paid {amount}! Total served: {customersServed}, Total earnings: {totalEarnings}");
    }
    
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
    
    [ContextMenu("Restart Game")]
    public void RestartGameCommand()
    {
        RestartGame();
    }
    
    #endregion
    
    private void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        
        // 게임 상태 정보
        GUILayout.Label("=== Restaurant Status ===");
        GUILayout.Label($"Game Running: {gameRunning}");
        GUILayout.Label($"Active Customers: {PoolManager.Instance.ActiveCustomerCount}");
        GUILayout.Label($"Available Seats: {CustomerSpawner.Instance.GetAvailableSeatCount()}/{CustomerSpawner.Instance.TotalSeatCount}");
        GUILayout.Label($"Customers Served: {customersServed}/{targetCustomersServed}");
        GUILayout.Label($"Total Earnings: {totalEarnings}");
        GUILayout.Label($"Game Time: {gameTime:F1}s / {gameTimeLimit}s");
        
        GUILayout.Space(10);
        
        // 조작 버튼들
        if (GUILayout.Button("Spawn Random Customer"))
        {
            if (customerSpawner != null)
                customerSpawner.SpawnSingleCustomer();
        }
        
        if (GUILayout.Button("Clear All Customers"))
        {
            if (customerSpawner != null)
                customerSpawner.ClearAllCustomers();
        }
        
        if (GUILayout.Button(gameRunning ? "Stop Game" : "Start Game"))
        {
            if (gameRunning)
                EndGame("수동 정지");
            else
                StartGame();
        }
        
        GUILayout.EndArea();
    }
}