using UnityEngine;

/// <summary>
/// 레스토랑 게임 매니저 (임시)
/// </summary>
public class RestaurantGameManager : Singleton<RestaurantGameManager>
{
    [Header("Managers")]
    [SerializeField] private CustomerSpawner customerSpawner;
    
    [Header("Game State")]
    [SerializeField] private bool gameRunning = true;
    [SerializeField] private int targetCustomersServed;

    [Header("Statistics")]
    [SerializeField] private int customersServed;
    [SerializeField] private int totalEarnings;

    //시간 UI 관련하여 수정
    [Tooltip("현재까지 경과한 시간")]
    [SerializeField] private float gameTime;

    [Header("라운드 시간 설정")]
    [Tooltip("1라운드(하루)의 제한 시간 (초 단위)")]
    [SerializeField] private float gameTimeLimit = 180f;

    //UI에 필요한 getter 추가
    public float CurrentGameTime => gameTime;
    public float GameTimeLimit => gameTimeLimit;
    public float TotalEarnings => totalEarnings;

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
    
    private void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 700));
        
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
            if (customerSpawner)
                customerSpawner.SpawnSingleCustomer();
        }
        
        if (GUILayout.Button("Clear All Customers"))
        {
            if (customerSpawner)
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
    
    public void StartGame()
    {
        gameRunning = true;
        gameTime = 0f;
        customersServed = 0;
        totalEarnings = 0;
        
        if (customerSpawner)
        {
            customerSpawner.StartSpawning();
        }

        //라운드 타이머 UI 표시 요청
        EventBus.Raise(UIEventType.ShowRoundTimer);

        Debug.Log("Restaurant game started!");
    }
    
    public void EndGame(string reason)
    {
        gameRunning = false;
        
        if (customerSpawner)
        {
            customerSpawner.StopSpawning();
        }

        //라운드 타이머 UI 숨기기
        EventBus.Raise(UIEventType.HideRoundTimer);


        Debug.Log($"Game ended: {reason}");
        Debug.Log($"Final Stats - Served: {customersServed}, Earnings: {totalEarnings}, Time: {gameTime:F1}s");
    }
    
    public void RestartGame()
    {
        // 모든 손님 정리
        if (customerSpawner)
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
        //이벤트 호출
        EventBus.Raise(UIEventType.UpdateEarnings, totalEarnings);
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
}