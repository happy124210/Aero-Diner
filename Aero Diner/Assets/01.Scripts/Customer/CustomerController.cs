using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Model과 View를 연결하고 상태 관리 및 외부 시스템과의 상호작용을 담당
/// </summary>
public class CustomerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private NavMeshAgent navAgent;
    private const float AGENT_DRIFT = 0.0001f;
    private const float ARRIVAL_THRESHOLD = 0.5f;
    private const float VELOCITY_THRESHOLD = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;
    public CustomerStateName CurrentStateName => currentState?.Name ?? default;

    // 상태 관리
    private Customer model;
    private CustomerView view;
    private CustomerState currentState; // 상태머신의 State
    
    #region Unity Functions
    
    private void Awake()
    {
        model = new Customer();
        view = GetComponent<CustomerView>();
        navAgent = GetComponent<NavMeshAgent>();
        
        if (!view) Debug.LogError($"[CustomerController]: {gameObject.name} CustomerView 없음 !!!");
        if (!navAgent) Debug.LogError($"[CustomerController]: {gameObject.name} NavMeshAgent 없음 !!!");
    }
    
    private void Update()
    {
        if (currentState == null) return;
        
        UpdateTimers(Time.deltaTime);
        UpdateAnimation();
        
        var nextState = currentState?.Update(this);
        if (nextState != null && nextState.Name != currentState.Name)
        {
            ChangeState(nextState);
        }
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromModelEvents();
    }
    
    #endregion

    private void ChangeState(CustomerState newState)
    {
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 상태 변경: {currentState?.Name} -> {newState?.Name}");
        
        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);
    }
    
    #region 초기화 & 이벤트 구독
    private void InitializeMVC()
    {
        view.Initialize();
        SubscribeToModelEvents();
    }

    private void SubscribeToModelEvents()
    {
        model.OnPatienceChanged += HandlePatienceChanged;
        model.OnPatienceStateChanged += HandlePatienceStateChanged;
        model.OnOrderPlaced += HandleOrderPlaced;
        model.OnMenuServed += HandleMenuServed;
        model.OnEatingFinished += HandleEatingFinished;
        model.OnPaymentEnd += HandlePaymentEnd;
    }

    private void UnsubscribeFromModelEvents()
    {
        model.OnPatienceChanged -= HandlePatienceChanged;
        model.OnPatienceStateChanged -= HandlePatienceStateChanged;
        model.OnOrderPlaced -= HandleOrderPlaced;
        model.OnMenuServed -= HandleMenuServed;
        model.OnEatingFinished -= HandleEatingFinished;
        model.OnPaymentEnd -= HandlePaymentEnd;
    }
    
    private void SetupNavMeshAgent(float speed)
    {
        if (!navAgent) return;
        
        navAgent.updateRotation = false;
        navAgent.updateUpAxis = false;
        navAgent.speed = speed;
        navAgent.stoppingDistance = 0.1f;
        navAgent.angularSpeed = 120f;
        navAgent.acceleration = 8f;
    }
    #endregion
    
    #region Model Event Handlers (Controller가 Model 변경사항을 View에 전달)
    
    private void HandlePatienceChanged(float currentPatience)
    {
        view.UpdatePatienceUI(model.GetPatienceRatio());
    }

    public void SetPatienceTimerActive(bool isActive)
    {
        model.SetPatienceTimerActive(isActive);
    }

    private void HandlePatienceStateChanged(bool isDecreasing)
    {
        view.UpdatePatienceVisibility(isDecreasing);
    }

    private void HandleOrderPlaced(FoodData order)
    {
        view.ShowOrderBubble(order);
        EventBus.Raise(UIEventType.ShowOrderPanel, model);
    }

    private void HandleMenuServed()
    {
        view.OnServedStateChanged();
        ChangeState(new EatingState());
    }

    private void HandleEatingFinished()
    {
        ChangeState(new PayingState());
    }

    private void HandlePaymentEnd(bool isCompleted)
    {
        view.OnPaymentStateChanged(isCompleted);
    }
    
    #endregion

    #region Customer Actions
    
    private void UpdateTimers(float deltaTime)
    {
        // 인내심 타이머
        if (ShouldPatienceDecrease())
        {
            float newPatience = Mathf.Max(0, model.RuntimeData.CurrentPatience - deltaTime);
            model.UpdatePatience(newPatience);
        }
    }
    
    public void UpdateQueuePosition(Vector3 newPosition)
    {
        SetDestination(newPosition);
    }

    public void MoveToAssignedSeat()
    {
        ChangeState(new MovingToSeatState());
    }

    public void SetAssignedTable(Table table)
    {
        model.SetAssignedTable(table);
    }
    
    public void PlaceOrder()
    {
        // TODO: 이벤트 연결
        RestaurantManager.Instance.OnCustomerEntered();
        model.PlaceOrder();
    }

    public void ReceiveFood(FoodData servedMenu)
    {
        if (GetCurrentOrder() == servedMenu)
        {
            model.ReceiveFood(servedMenu);
            // TODO: view의 happy 이펙트  부르기
            if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 음식 서빙됨!");
        }
    }

    public void ProcessPayment()
    {
        // TODO: view의 결제 부르기
        RestaurantManager.Instance.OnCustomerPaid(GetCurrentOrder().foodCost);
        EventBus.Raise(UIEventType.UpdateEarnings);
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 결제 시작!");
    }

    public void ForceLeave()
    {
        ChangeState(new LeavingState());
    }

    public void Despawn()
    {
        PoolManager.Instance.DespawnCustomer(this);
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 퇴장 및 비활성화");
    }
    #endregion

    #region IPoolable Implementation
    public void InitializeFromPool(CustomerData customerData)
    {
        if (!customerData)
        {
            Debug.LogError($"[CustomerController]: {gameObject.name} customerData가 null입니다!");
            return;
        }
        
        model.Initialize(customerData);
        InitializeMVC();
        SetupNavMeshAgent(model.Data.speed);
        
        ChangeState(new MovingToEntranceState());
        
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 풀에서 초기화 완료 - {customerData.customerName}");
    }

    public void OnReturnToPool()
    {
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 풀로 반환");

        // 정리 작업
        currentState = null;
        UnsubscribeFromModelEvents();
        view.Cleanup();
        
        // NavMesh 정리
        if (navAgent && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
        }
    }
    #endregion
    
    #region Movement & Animation
    public void SetDestination(Vector3 destination)
    {
        if (!navAgent) return;
        
        navAgent.enabled = true;
        navAgent.isStopped = false;
        navAgent.SetDestination(destination);
    }

    public void StopMovement()
    {
        if (!navAgent || !navAgent.isOnNavMesh) return;
        
        navAgent.isStopped = true;
        navAgent.velocity = Vector3.zero;
    }

    public bool HasReachedDestination()
    {
        if (!navAgent || navAgent.enabled == false) return false;
        
        bool reached = navAgent.remainingDistance <= ARRIVAL_THRESHOLD 
                                 && navAgent.velocity.sqrMagnitude < VELOCITY_THRESHOLD * VELOCITY_THRESHOLD;
        
        return reached;
    }
    
    public void AdjustToSeatPosition()
    {
        navAgent.enabled = false;
        transform.position = GetSeatPosition();
    }
    
    public void AdjustToStopPosition()
    {
        navAgent.enabled = false;
        transform.position = GetStopPosition();
    }
    
    private void UpdateAnimation()
    {
        if (!navAgent || !view) return;
        
        Vector3 localVelocity = transform.InverseTransformDirection(navAgent.velocity);
        Vector2 direction = new Vector2(localVelocity.x, localVelocity.z).normalized;
    
        view.UpdateAnimationDirection(direction);
    }
    public void SetAnimationState(CustomerAnimState state)
    {
        view.SetAnimationState(state);
    }
    
    #endregion
    
    # region property & public getters
    
    public CustomerData CustomerData => model.Data;
    public float GetEatingTime() => model.Data.eatTime;
    public Table GetAssignedTable() => model.RuntimeData.AssignedTable;
    public FoodData GetCurrentOrder() => model.RuntimeData.CurrentOrder;
    public Vector3 GetStopPosition() => GetAssignedTable().GetStopPoint();
    public Vector3 GetSeatPosition() => GetAssignedTable().GetSeatPoint();
    
    public bool HasPatience() => model.RuntimeData.CurrentPatience > 0;
    private bool ShouldPatienceDecrease() => currentState != null && (currentState.Name == CustomerStateName.Ordering || currentState.Name == CustomerStateName.WaitingInLine);
    
    #endregion
}