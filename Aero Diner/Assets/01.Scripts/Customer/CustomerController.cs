using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// MVC 패턴의 Controller
/// Model과 View를 연결하고 상태 관리 및 외부 시스템과의 상호작용을 담당
/// </summary>
public class CustomerController : MonoBehaviour, IPoolable
{
    [Header("Movement")]
    [SerializeField] private NavMeshAgent navAgent;
    private const float AGENT_DRIFT = 0.0001f;
    private const float ARRIVAL_THRESHOLD = 0.5f;
    private const float VELOCITY_THRESHOLD = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;

    // 상태 관리
    private CustomerState currentState;
    private CustomerEventBridge bridge;
   
    private Customer model;
    private CustomerView view;
    
    #region Public Getters & methods (Model 데이터 접근)
    
    public CustomerData CurrentData => model.CustomerData;
    public FoodData CurrentOrder => model.CurrentOrder;
    public float PatienceRatio => model.CurrentPatience / model.MaxPatience;
    public bool HasPatience() => model.HasPatience;
    public Table GetAssignedTable() => model.AssignedTable;
    public Vector3 GetAssignedStopPosition() => model.AssignedTable?.GetStopPosition() ?? Vector3.zero;
    
    public bool IsFoodServed() => model.IsServed;
    public bool IsCustomerEating() => model.IsEating;
    public bool IsPaymentCompleted() => model.IsPaymentCompleted;
    
    #endregion
    
    #region Unity Events
    private void Awake()
    {
        // MVC 컴포넌트 초기화
        model = new Customer();
        view = GetComponent<CustomerView>();
        
        if (!view)
        {
            view = gameObject.AddComponent<CustomerView>();
        }
        
        bridge = new CustomerEventBridge();
        bridge.Bind(model);
        
        // NavMesh
        navAgent = GetComponent<NavMeshAgent>();
        if (!navAgent) Debug.LogError($"[CustomerView]: {gameObject.name} NavMeshAgent 없음!");
    }

    private void Start()
    {
        if (model.CustomerData != null)
        {
            InitializeMVC();
            ChangeState(new MovingToEntranceState());
        }
        SetupNavMeshAgent(CurrentData.speed);
    }
    
    private void Update()
    {
        if (model.HasLeftRestaurant) return;

        // Model 업데이트
        model.UpdatePatience(Time.deltaTime);
        model.UpdateEatingTimer(Time.deltaTime);

        // 상태 관리
        if (currentState != null)
        {
            CustomerState nextState = currentState.Update(this);
            
            if (nextState != currentState)
                ChangeState(nextState);
        }
    }
    
    #endregion

    #region 초기화 & 이벤트 구독
    private void InitializeMVC()
    {
        view.Initialize(model.Speed);
        SubscribeToModelEvents();
    }

    private void SubscribeToModelEvents()
    {
        model.OnPatienceChanged += HandlePatienceChanged;
        model.OnPatienceStateChanged += HandlePatienceStateChanged;
        model.OnOrderPlaced += HandleOrderPlaced;
        model.OnServedStateChanged += HandleServedStateChanged;
        model.OnEatingStateChanged += HandleEatingStateChanged;
        model.OnPaymentStateChanged += HandlePaymentStateChanged;
    }

    private void UnsubscribeFromModelEvents()
    {
        model.OnPatienceChanged -= HandlePatienceChanged;
        model.OnPatienceStateChanged -= HandlePatienceStateChanged;
        model.OnOrderPlaced -= HandleOrderPlaced;
        model.OnServedStateChanged -= HandleServedStateChanged;
        model.OnEatingStateChanged -= HandleEatingStateChanged;
        model.OnPaymentStateChanged -= HandlePaymentStateChanged;
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
    
    private void ChangeState(CustomerState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);
        
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 상태 변경 {currentState?.StateName}");
    }

    #region Model Event Handlers (Controller가 Model 변경사항을 View에 전달)
    
    private void HandlePatienceChanged(float currentPatience)
    {
        view.UpdatePatienceUI(PatienceRatio);
    }

    private void HandlePatienceStateChanged(bool isDecreasing)
    {
        view.UpdatePatienceVisibility(isDecreasing);
    }

    private void HandleOrderPlaced(FoodData order)
    {
        view.ShowOrderBubble(order);
    }

    private void HandleServedStateChanged(bool isServed)
    {
        view.OnServedStateChanged(isServed);

    }

    private void HandleEatingStateChanged(bool isEating)
    {
        // Timer 시작 알림?
    }

    private void HandlePaymentStateChanged(bool isCompleted)
    {
        view.OnPaymentStateChanged(isCompleted);
    }

    private void HandleAnimationStateChanged(CustomerAnimState state)
    {
        view.SetAnimationState(state);
    }
    
    #endregion

    #region Customer Actions
    public void UpdateQueuePosition(Vector3 newPosition)
    {
        SetDestination(newPosition);
    }

    public void StartWaitingInLine(Vector3 queuePosition)
    {
        SetDestination(queuePosition);
        ChangeState(new WaitingInLineState());
    }

    public void MoveToAssignedSeat()
    {
        ChangeState(new MovingToSeatState());
    }

    public void SetAssignedTable(Table table)
    {
        model.SetAssignedTable(table);
    }

    public void AdjustSeatPosition()
    {
        transform.position = GetAssignedTable().transform.position;
        view.SetAnimationState(CustomerAnimState.Idle);
    }

    public void PlaceOrder()
    {
        RestaurantManager.Instance.OnCustomerEntered();
        model.PlaceOrder();
    }

    public void ReceiveFood(FoodData servedMenu)
    {
        model.ReceiveFood(servedMenu);
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 음식 서빙됨!");
    }

    public void StartEating()
    {
        model.StartEating();
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 식사 시작");
    }

    public void ProcessPayment()
    {
        model.ProcessPayment();
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 결제 완료!");
    }

    public void ForceLeave()
    {
        model.StopPatienceTimer();
        ChangeState(new LeavingState());
    }

    public void Despawn()
    {
        if (model.HasLeftRestaurant) return;
        
        model.SetLeftRestaurant();
        
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 떠남");
        
        CancelInvoke();
        PoolManager.Instance.DespawnCustomer(this);
    }
    #endregion

    #region Customer UI Controls
    public void StartPatienceTimer()
    {
        model.StartPatienceTimer();
    }

    public void StopPatienceTimer()
    {
        model.StopPatienceTimer();
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
        ChangeState(new MovingToEntranceState());
        
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 풀에서 초기화 완료 - {customerData.customerName}");
    }

    public void OnGetFromPool()
    {
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 풀에서 가져옴");
    }

    public void OnReturnToPool()
    {
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 풀로 반환");

        // 정리 작업
        UnsubscribeFromModelEvents();
        view.Cleanup();
        model.ResetData();
        ChangeState(null);
        
        // TODO: Customer Manager
        TableManager.Instance.RemoveCustomerFromQueue(this);
        TableManager.Instance.ReleaseSeat(this);
        
        // NavMesh 정리
        if (navAgent && navAgent.isOnNavMesh)
        {
            navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
        }
        
        CancelInvoke();
        StopAllCoroutines();
    }

    public void OnDestroyFromPool()
    {
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 풀에서 삭제");

        UnsubscribeFromModelEvents();
        CancelInvoke();
        StopAllCoroutines();
    }
    #endregion
    
    #region Movement & Animation
    public void SetDestination(Vector3 destination)
    {
        if (!navAgent || !navAgent.isOnNavMesh)
        {
            if (showDebugInfo) Debug.LogWarning($"[CustomerView]: {gameObject.name} NavMesh 문제!");
            return;
        }

        navAgent.isStopped = false;
        
        // NavMeshPlus Y축 버그 방지
        if (Mathf.Abs(transform.position.x - destination.x) < AGENT_DRIFT)
        {
            destination.x += AGENT_DRIFT;
        }

        navAgent.SetDestination(destination);
        
        if (showDebugInfo) Debug.Log($"[CustomerView]: {gameObject.name} 목적지 설정: {destination}");
    }

    public void StopMovement()
    {
        if (!navAgent) return;
        
        navAgent.isStopped = true;
        navAgent.ResetPath();
        navAgent.velocity = Vector3.zero;
    }

    public bool HasReachedDestination()
    {
        if (!navAgent || !navAgent.isOnNavMesh) return false;

        bool reached = !navAgent.pathPending && 
                       navAgent.remainingDistance < ARRIVAL_THRESHOLD && 
                       navAgent.velocity.sqrMagnitude < VELOCITY_THRESHOLD;

        if (reached && showDebugInfo) 
            Debug.Log($"[CustomerView]: {gameObject.name} 목적지 도착!");

        return reached;
    }
    
    #endregion
    
}