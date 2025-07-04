using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// 손님 애니메이션 상태
/// </summary>
public enum CustomerAnimState
{
    Idle,
    Walking,
}

public class CustomerController : MonoBehaviour, IPoolable
{
    [Header("Customer Stats")]
    [SerializeField] private CustomerData currentData;
    private float speed;
    private float maxPatience;
    private float eatTime;
    private float currentPatience;
    private Table assignedTable;
    
    [Header("Order")]
    [SerializeField] private FoodData currentOrder;
    
    [Header("Customer UI")]
    [SerializeField] private Canvas customerUI;
    [SerializeField] private Image orderBubble;
    [SerializeField] private Image patienceTimer;
    private bool isPatienceDecreasing;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;
    [SerializeField] public string currentNodeName;
    
    // 상태 체크용 bool변수들
    private bool isServed;
    private bool isEating;
    private bool isEatingFinished;
    private bool isPaymentCompleted;
    private bool hasLeftRestaurant;

    // components
    private NavMeshAgent navAgent;
    private CustomerState currentState;
    private float eatingTimer;


#region Unity Events

    private void Reset()
    {
        customerUI = transform.FindChild<Canvas>("Group_Customer");
        orderBubble = transform.FindChild<Image>("Img_OrderBubble");
        patienceTimer = transform.FindChild<Image>("Img_PatienceTimer");    
    }

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError($"[CustomerController]: {gameObject.name} NavMeshAgent 없음 !!!");
        }
        
        customerUI = transform.FindChild<Canvas>("Group_Customer");
        orderBubble = transform.FindChild<Image>("Img_OrderBubble");
        patienceTimer = transform.FindChild<Image>("Img_PatienceTimer");
    }

    private void Start()
    {
        SetupNavMeshAgent();
        SetupCustomerData();
        ChangeState(new MovingToEntranceState());
    }

    private void Update()
    {
        if (hasLeftRestaurant) return;
        
        if (isPatienceDecreasing)
            currentPatience -= Time.deltaTime;
        
        UpdateCustomerUI();
        
        if (currentState != null)
        {
            CustomerState nextState = currentState.Update(this);
            
            if (nextState != currentState)
                ChangeState(nextState);
        }
        
        // 식사 중 타이머 처리
        if (isEating)
        {
            eatingTimer += Time.deltaTime;
            if (eatingTimer >= eatTime)
            {
                isEating = false;
                isEatingFinished = true;
                if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 식사 완료!");
            }
        }
    }

#endregion
    
#region Setup Functions
    
    /// <summary>
    /// 외부에서 받는 손님 데이터 셋업
    /// </summary>
    private void SetupCustomerData()
    {
        if (!currentData)
        {
            Debug.LogError($"[CustomerController]: {gameObject.name} currentData가 null입니다!");
            return;
        }
        
        speed = currentData.speed;
        maxPatience = currentData.waitTime;
        eatTime = currentData.eatTime;
        
        currentPatience = maxPatience;
        
        ResetCustomerData();
        
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 데이터 셋업 완료 - 속도: {speed}, 인내심: {maxPatience}");
    }

    /// <summary>
    /// 손님 상태 초기화
    /// </summary>
    private void ResetCustomerData()
    {
        isServed = false;
        isEating = false;
        isEatingFinished = false;
        isPaymentCompleted = false;
        hasLeftRestaurant = false;
        eatingTimer = 0f;
        
        // UI 초기화
        HideAllUI();
        isPatienceDecreasing = false;
        
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 데이터 리셋 완료");
    }
    
    /// <summary>
    /// NavMesh 필드 셋업
    /// </summary>
    private void SetupNavMeshAgent()
    {
        if (!navAgent)
        {
            Debug.LogError($"[CustomerController]: {gameObject.name} NavMeshAgent가 없습니다!");
            return;
        }

        // 2D NavMesh 설정
        navAgent.updateRotation = false;
        navAgent.updateUpAxis = false;
        navAgent.speed = speed;
        navAgent.stoppingDistance = 0.1f;
        navAgent.angularSpeed = 120f;
        navAgent.acceleration = 8f;
        
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} NavMeshAgent 셋업 완료");
    }

    private void ChangeState(CustomerState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);
        
        if (showDebugInfo) Debug.Log($"Customer state changed to: {currentState?.StateName}");
    }

    public void SetCurrentNodeName(string newNodeName)
    {
        currentNodeName = newNodeName;
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} Current node: {currentNodeName}");
    }
    
#endregion
    
#region Customer Actions & State
    
    /// <summary>
    /// 줄 위치 업데이트
    /// </summary>
    public void UpdateQueuePosition(Vector3 newPosition)
    {
        SetDestination(newPosition);
    }
    
    /// <summary>
    /// 줄서기 시작
    /// </summary>
    public void StartWaitingInLine(Vector3 queuePosition)
    {
        SetDestination(queuePosition);
        ChangeState(new WaitingInLineState());
    }

    /// <summary>
    /// 할당된 좌석으로 이동
    /// </summary>
    public void MoveToAssignedSeat()
    {
        ChangeState(new MovingToSeatState());
    }
    
    /// <summary>
    /// 할당된 테이블 설정
    /// </summary>
    public void SetAssignedTable(Table table)
    {
        assignedTable = table;
    }

    /// <summary>
    /// 할당된 좌석 위치 반환
    /// </summary>
    public Vector3 GetAssignedSeatPosition()
    {
        return assignedTable ? assignedTable.GetSeatPosition() : Vector3.zero;
    }
    
    public void PlaceOrder()
    {
        RestaurantManager.Instance.OnCustomerEntered();
        currentOrder = MenuManager.Instance.GetRandomMenu();
        orderBubble.sprite = currentOrder.foodIcon;
        ShowOrderBubble();
    }
    
    public void ReceiveFood(FoodData servedMenu)
    {
        if (isServed) return;

        if (currentOrder.id == servedMenu.id)
        {
            isServed = true;
        }
        
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 음식 서빙됨!");
    }
    
    public bool IsFoodServed() => isServed;
    
    public void StartEating()
    {
        isEating = true;
        eatingTimer = 0f;
        isEatingFinished = false;
        SetAnimationState(CustomerAnimState.Idle);
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 식사 시작");
    }
    
    public bool IsEatingFinished() => isEatingFinished;
    public bool IsPaymentCompleted() => isPaymentCompleted;
    
    public void ProcessPayment()
    {
        int payment = Mathf.RoundToInt(currentOrder.foodCost);
        RestaurantManager.Instance.OnCustomerPaid(payment);
        isPaymentCompleted = true;
        
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} {payment} 코인 결제!");
        // TODO: 결제 이펙트
    }
    
#endregion

#region Movement & Animation

    private const float AGENT_DRIFT = 0.0001f;
    
    /// <summary>
    /// 목적지 설정
    /// </summary>
    public void SetDestination(Vector3 destination) 
    { 
        if (!navAgent)
        {
          //  Debug.LogError($"[CustomerController]: {gameObject.name} NavMeshAgent가 null입니다!");
            return;
        }
        
        if (!navAgent.isOnNavMesh)
        {
          //  Debug.LogWarning($"[CustomerController]: {gameObject.name}이 NavMesh 위에 있지 않습니다!");
            return;
        }
        
        // NavMeshPlus Y축 버그 방지용
        if (Mathf.Abs(transform.position.x - destination.x) < AGENT_DRIFT)
        {
            destination.x += AGENT_DRIFT;
        }
        
        navAgent.SetDestination(destination);
        SetAnimationState(CustomerAnimState.Walking);
        
        //if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 목적지 설정: {destination}");
    }
    
    /// <summary>
    /// 목적지 도달 체크
    /// </summary>
    public bool HasReachedDestination()
    {
        const float arrivalThreshold = 0.5f;
        const float velocityThreshold = 0.1f;
        
        if (!navAgent || !navAgent.isOnNavMesh) 
        {
        //    if (showDebugInfo) Debug.LogWarning($"[CustomerController]: {gameObject.name} NavMeshAgent 문제!");
            return false;
        }
        
        bool reached = !navAgent.pathPending && 
                      navAgent.remainingDistance < arrivalThreshold && 
                      navAgent.velocity.sqrMagnitude < velocityThreshold;
        
       // if (reached && showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 목적지 도착!");
            
        return reached;
    }
    
    public void SetAnimationState(CustomerAnimState state) 
    { 
        // TODO: 실제 애니메이터 연동
       // if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 애니메이션 상태: {state}");
    }
    
    /// <summary>
    /// Despawn
    /// </summary>
    public void Despawn()
    {
        if (hasLeftRestaurant) return;
        
        hasLeftRestaurant = true;
        
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 떠남");
        
        // 예약된 Invoke 취소
        CancelInvoke();
        
        PoolManager.Instance.DespawnCustomer(this);
    }
    
    #endregion
    
#region Customer UI
    
    /// <summary>
    /// 손님 UI 업데이트
    /// </summary>
    private void UpdateCustomerUI()
    {
        UpdatePatienceTimerUI();
        
        // TODO: 만족도 UI 등
    }
    
    /// <summary>
    /// 인내심 타이머 UI 업데이트
    /// </summary>
    private void UpdatePatienceTimerUI()
    {
        if (!patienceTimer) return;
        
        if (isPatienceDecreasing)
        {
            ShowPatienceTimer();

            float patienceRatio = currentPatience / maxPatience;
            
            // 이미지 변경
            patienceTimer.fillAmount = patienceRatio;
            Color timerColor = patienceRatio switch
            {
                > 0.66f => Color.green,
                > 0.33f => Color.yellow,
                _ => Color.red
            };
            patienceTimer.color = timerColor;
        }
        else
        {
            HidePatienceTimer();
        }
    }
    
    /// <summary>
    /// 인내심 타이머 표시
    /// </summary>
    private void ShowPatienceTimer()
    {
        customerUI.gameObject.SetActive(true);
        patienceTimer.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// 인내심 타이머 숨기기
    /// </summary>
    private void HidePatienceTimer()
    {
        patienceTimer.gameObject.SetActive(false);
        orderBubble.gameObject.SetActive(false);
        customerUI.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 주문창 표시
    /// </summary>
    private void ShowOrderBubble()
    {
        customerUI.gameObject.SetActive(true);
        orderBubble.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// 모든 UI 숨기기
    /// </summary>
    private void HideAllUI()
    {
        HidePatienceTimer();
        customerUI.gameObject.SetActive(false);
    }
    
    // public
    public void StartPatienceTimer()
    {
        isPatienceDecreasing = true;
        ShowPatienceTimer();
    }

    public void StopPatienceTimer()
    {
        isPatienceDecreasing = false;
        currentPatience = maxPatience;
        HidePatienceTimer();
    }
    
    #endregion

#region IPoolable

    /// <summary>
    /// 풀에서 가져온 후 데이터로 초기화
    /// </summary>
    public void InitializeFromPool(CustomerData customerData)
    {
        if (!customerData)
        {
            Debug.LogError($"[CustomerController]: {gameObject.name} customerData가 null입니다!");
            return;
        }
        
        currentData = customerData;
        hasLeftRestaurant = false;
        
        SetupNavMeshAgent();
        SetupCustomerData();
        ChangeState(new MovingToEntranceState());
        
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 풀에서 초기화 완료 - {customerData.customerName}");
    }
    
    public void OnGetFromPool()
    {
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 풀에서 가져옴");
    }

    /// <summary>
    /// 풀로 반환 시 정리
    /// </summary>
    public void OnReturnToPool()
    {
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 풀로 반환");
        
        HideAllUI();
        
        // 예약된 작업들 취소
        CancelInvoke();
        StopAllCoroutines();
        
        // BT 정리
        ChangeState(new MovingToEntranceState());
        
        // NavMeshAgent 정리
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
        }
        
        // 대기/좌석 정리
        TableManager.Instance.RemoveCustomerFromQueue(this);
        TableManager.Instance.ReleaseSeat(this);

        // Animation 정리
        SetAnimationState(CustomerAnimState.Idle);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
        // 데이터 초기화
        ResetCustomerData();
        currentData = null;
    }

    public void OnDestroyFromPool()
    {
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 풀에서 삭제");

        CancelInvoke();
        StopAllCoroutines();
    }

    #endregion
    
#region public Getters
    
    public CustomerData CurrentData => currentData;
    public bool HasPatience() => currentPatience > 0;
    public Table GetAssignedTable() => assignedTable;
    public FoodData CurrentOrder => currentOrder;
    
    #endregion
    
}