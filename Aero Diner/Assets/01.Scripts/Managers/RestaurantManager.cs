using System.Collections;
using UnityEngine;

/// <summary>
/// 레스토랑 운영 총괄
/// GameManager로부터 Operation Phase 신호를 받아 실제 운영을 담당
/// </summary>
public class RestaurantManager : Singleton<RestaurantManager>
{
    [Tooltip("하루 제한 시간 (초 단위)")]
    [SerializeField] private float roundTimeLimit = 300f;

    [Header("레이아웃")]
    [SerializeField] private Transform entrancePoint;
    [SerializeField] private Transform exitPoint;
    
    [Header("실시간 통계")]
    [SerializeField] private int customersServed;
    [SerializeField] private int customersVisited;
    [SerializeField] private int todayEarnings;
    
    [Header("Debug Info")]
    [SerializeField] private CustomerSpawner customerSpawner;
    [SerializeField] private bool showDebugInfo;
    
    // private fields
    
    private float currentRoundTime;
    
    #region property
    
    public float CurrentRoundTime => currentRoundTime;
    public float RoundTimeLimit => roundTimeLimit;
    public int CustomersServed => customersServed;
    public int CustomersVisited => customersVisited;
    public int TodayEarnings => todayEarnings;
    
    #endregion

    #region Unity events

    protected override void Awake()
    {
        base.Awake();
        if (!customerSpawner) customerSpawner = transform.GetComponent<CustomerSpawner>();

        EventBus.OnGameEvent += HandleGameEvent;
    }

    private void OnDestroy()
    {
        EventBus.OnGameEvent -= HandleGameEvent;
    }

    // 영업 중 타이머 계산
    private void Update()
    {
        if (GameManager.Instance.CurrentPhase != GamePhase.Operation) return;
        
        currentRoundTime -= Time.deltaTime;

        if (currentRoundTime <= 0)
        {
            EventBus.Raise(GameEventType.RoundTimerEnded);
            currentRoundTime = -1f; 
        }
    }

    #endregion

    #region 이벤트 핸들링
    
    private void HandleGameEvent(GameEventType eventType, object data)
    {
        if (eventType != GameEventType.GamePhaseChanged) return;

        GamePhase newPhase = (GamePhase)data;
        
        switch (newPhase)
        {
            // 영업 시작 준비
            case GamePhase.Opening:
                InitializeDay();
                currentRoundTime = roundTimeLimit;
                EventBus.Raise(UIEventType.ShowRoundTimer);
                // TODO: 이벤트 체크
                
                if (showDebugInfo) Debug.Log("[RestaurantManager] Opening: 영업 준비 시작");
                break;

            // 영업 시작
            case GamePhase.Operation:
                customerSpawner.StartSpawning();
                
                if (showDebugInfo) Debug.Log("[RestaurantManager] Operation: 손님 스폰 시작");
                break;

            // 마감 시작
            case GamePhase.Closing:
                customerSpawner.StopSpawning();
                StartCoroutine(CleanupAndShowResult());

                if (showDebugInfo) Debug.Log("[RestaurantManager] Closing: 손님 스폰 중단, 뒷정리 시작");
                break;
        }
    }

    #endregion

    #region 레스토랑 운영 로직

    /// <summary>
    /// 하루 영업 시작 전에 데이터 초기화
    /// </summary>
    private void InitializeDay()
    {
        customersServed = 0;
        customersVisited = 0;
        todayEarnings = 0;
    }

    /// <summary>
    /// 모든 손님이 퇴장할 때까지 기다린 후 결과 패널 표시
    /// </summary>
    private IEnumerator CleanupAndShowResult()
    {
        if (showDebugInfo) Debug.Log("[RestaurantManager]: 모든 손님이 떠나길 기다리는 중...");
        
        TableManager.Instance.ReleaseAllQueues();
        yield return new WaitUntil(() => CustomerManager.Instance.ActiveCustomerCount == 0);
        
        Debug.Log("[RestaurantManager]: 손님 다 나감. 결과창 표시");
        EventBus.Raise(UIEventType.ShowResultPanel, todayEarnings);
    }

    #endregion
    
    #region 데이터 변경 메서드

    public void OnCustomerEntered() => customersVisited++;
    public void OnCustomerServed() => customersServed++;
    
    public void AddDailyEarnings(int amount)
    {
        todayEarnings += amount;
    }

    #endregion

    #region public getters
    
    public Vector3 GetEntrancePoint() => entrancePoint.position;
    public Vector3 GetExitPoint() => exitPoint.position;
    
    #endregion
    
    #region Debug Commands
#if UNITY_EDITOR  
    private void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 700));
        GUILayout.Space(10);
        
        if (GameManager.Instance.CurrentPhase != GamePhase.Operation)
        {
            if (GUILayout.Button("영업 시작"))
            {
                GameManager.Instance.ChangePhase(GamePhase.Operation);
            }
        }
        else
        {
            if (GUILayout.Button("영업 마감"))
            {
                currentRoundTime = 0;
            }
        }
        
        if (GUILayout.Button("영업 강제 종료"))
        {
            currentRoundTime = 0;
            CustomerManager.Instance.EmptyAllPatience();
        }
        
        if (GUILayout.Button("손님 1명 스폰"))
        {
            if (customerSpawner) customerSpawner.SpawnSingleCustomer();
        }
        
        // TODO: 이동 필요
        if (GUILayout.Button("모든 메뉴 해금"))
        {
            MenuManager.Instance.UnlockAllMenus();
        }
        
        GUILayout.EndArea();
    }
#endif    
    #endregion
}