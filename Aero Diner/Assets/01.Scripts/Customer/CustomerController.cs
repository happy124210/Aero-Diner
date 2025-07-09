using UnityEngine;

/// <summary>
/// MVC 패턴의 Controller
/// Model과 View를 연결하고 상태 관리 및 외부 시스템과의 상호작용을 담당
/// </summary>
public class CustomerController : MonoBehaviour, IPoolable
{
    [Header("MVC Components")]
    [SerializeField] private Customer model;
    [SerializeField] private CustomerView view;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;
    [SerializeField] public string currentNodeName;

    // 상태 관리
    private CustomerState currentState;
    private CustomerEventBridge bridge;
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
    }

    private void Start()
    {
        if (model.CustomerData != null)
        {
            InitializeMVC();
            ChangeState(new MovingToEntranceState());
        }
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
        view.SetDestination(newPosition);
    }

    public void StartWaitingInLine(Vector3 queuePosition)
    {
        view.SetDestination(queuePosition);
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
        if (model.AssignedTable == null) return;
        view.AdjustSeatPosition(model.AssignedTable.SeatPoint.position);
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

    #region Movement & Animation (Controller -> View)
    public void SetDestination(Vector3 destination)
    {
        // View에 직접 명령
        view.SetDestination(destination);
        view.SetAnimationState(CustomerAnimState.Walking);
    }

    public void StopMovement()
    {
        view.StopMovement();
        view.SetAnimationState(CustomerAnimState.Idle);
    }

    public bool HasReachedDestination()
    {
        return view.HasReachedDestination();
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
    
}