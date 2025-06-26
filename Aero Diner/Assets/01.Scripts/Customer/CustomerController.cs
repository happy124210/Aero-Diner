using UnityEngine;
using UnityEngine.AI;
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
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;
    [SerializeField] public string currentNodeName;
    
    [Header("Customer Stats")]
    [SerializeField] private CustomerData currentData;
    private float speed;
    private float maxWaitTime;
    private float eatTime;
    private float currentPatience;
    
    [Header("Positions - 임시")]
    [SerializeField] private Transform entrancePoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private Transform seatPoint;
    
    private INode rootNode;
    private Vector3 assignedSeatPosition;
    private float eatingTimer;
    
    // 상태 체크용 bool변수들
    private bool foodServed;
    private bool isEating;
    private bool eatingFinished;
    private bool paymentCompleted;

    // components
    private NavMeshAgent navAgent;
    
    #region Unity Events

    private void Awake()
    {
        // 임시
        entrancePoint = transform.Find("Entrance Point");
        exitPoint = transform.Find("Exit Point");
        seatPoint = transform.Find("Approach Position");
    }

    private void Start()
    {
        SetupCustomerData();
        SetupNavMeshAgent();
        SetupBT();
    }

    private void Update()
    {
        // 결제 완료 전까지만 인내심 감소
        if (currentPatience > 0 && !paymentCompleted)
        {
            currentPatience -= Time.deltaTime;
        }
        
        // 식사 중일 때 타이머 처리
        if (isEating)
        {
            eatingTimer += Time.deltaTime;
            if (eatingTimer >= eatTime)
            {
                isEating = false;
                eatingFinished = true;
            }
        }
        
        // BT 실행
        if (rootNode != null)
        {
            NodeState state = rootNode.Execute();

            if (state == NodeState.Success || state == NodeState.Failure)
            {
                if (showDebugInfo)
                    Debug.Log($"Customer BT completed with state: {state}");
            }
        }
    }

    #endregion
    
    /// <summary>
    /// 데이터로부터 손님 데이터 셋업
    /// </summary>
    private void SetupCustomerData()
    {
        speed = currentData.speed; 
        maxWaitTime = currentData.waitTime;
        eatTime = currentData.eatTime;
        
        currentPatience = maxWaitTime;
    }

    /// <summary>
    /// 손님 데이터 초기화
    /// </summary>
    private void ResetCustomerData()
    {
        foodServed = false;
        isEating = false;
        eatingFinished = false;
        paymentCompleted = false;
        eatingTimer = 0f;
        assignedSeatPosition = Vector3.zero;
        currentPatience = 0f;
    }
    
    /// <summary>
    /// NavMesh 필드 셋업
    /// </summary>
    private void SetupNavMeshAgent()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        // 2D NavMesh
        navAgent.updateRotation = false;
        navAgent.updateUpAxis = false;
        navAgent.speed = speed;
        navAgent.stoppingDistance = 0.1f;
    }
    
    private void SetupBT()
    {
        // 전체 고객 흐름을 하나의 Sequence로 구성
        var mainFlow = new Sequence(this,
            new MoveToEntrance(this),
            new CheckWaitingTime(this),
            new CheckAvailableSeat(this),
            new MoveToSeat(this),
            new Selector(this,
                new Sequence(this,
                    new CheckWaitingTime(this),
                    new TakeOrder(this),
                    new Payment(this)
                ),
                new Leave(this) // 중간에 인내심 소진시 이탈
            ),
            new Leave(this) // 정상 완료 후 이탈
        );
    
        // 전체 실패시에도 이탈 처리
        rootNode = new Selector(this,
            mainFlow,
            new Leave(this)
        );
    
        rootNode.Reset();
    
        if (showDebugInfo)
            Debug.Log("Customer BT setup completed");
    }

    public void SetCurrentNodeName(string newNodeName)
    {
        currentNodeName = newNodeName;
        if (showDebugInfo)
            Debug.Log($"Current node: {currentNodeName}");
    }
    
    #region Customer Actions & State
    
    public float GetRemainingPatience() => currentPatience;
    
    public bool HasAvailableSeat()
    {
        // 임시로 좌석 체크
        return CustomerSpawner.Instance.AssignSeatToCustomer(this);
    }
    
    /// <summary>
    /// CustomerSpawner에서 할당된 좌석 위치 설정
    /// </summary>
    public void SetAssignedSeatPosition(Vector3 seatPosition)
    {
        assignedSeatPosition = seatPosition;
    
        if (showDebugInfo)
            Debug.Log($"Customer assigned to seat at {seatPosition}");
    }
    
    public Vector3 GetAssignedSeatPosition() => assignedSeatPosition;
    
    public void PlaceOrder()
    {
        // TODO: 실제 주문 시스템과 연동
        Debug.Log("Order placed!");
        
        // 임시로 2-5초 후 음식 서빙
        Invoke(nameof(ServeFood), Random.Range(2f, 5f));
    }
    
    private void ServeFood()
    {
        foodServed = true;
        Debug.Log("Food served to customer!");
    }
    
    public bool IsFoodServed() => foodServed;
    
    public void StartEating()
    {
        isEating = true;
        eatingTimer = 0f;
        eatingFinished = false;
        SetAnimationState(CustomerAnimState.Idle);
        Debug.Log("Customer started eating");
    }
    
    public bool IsEatingFinished() => eatingFinished;
    public bool IsPaymentCompleted() => paymentCompleted;
    
    public void ProcessPayment()
    {
        // TODO: 실제 결제 시스템과 연동
        int payment = Random.Range(100, 500);
        Debug.Log($"Customer paid {payment} coins!");
        
        // 결제 완료 표시 (더 이상 인내심 감소 안함)
        paymentCompleted = true;
        
        // TODO: 결제 이펙트
    }
    
    #endregion
    
    #region Movement & Animation

    private const float AGENT_DRIFT = 0.0001f;
    public void SetDestination(Vector3 destination) 
    { 
        if (navAgent && navAgent.isOnNavMesh)
        {
            // NavMeshPlus Y축 버그 방지용
            if (Mathf.Abs(transform.position.x - destination.x) < AGENT_DRIFT)
            {
                destination.x += AGENT_DRIFT;
            }
            
            navAgent.SetDestination(destination);
            SetAnimationState(CustomerAnimState.Walking);
        }
    }
    
    public bool HasReachedDestination() 
    { 
        if (!navAgent || !navAgent.isOnNavMesh) return false;
        
        return !navAgent.pathPending && 
               navAgent.remainingDistance < 0.5f && 
               navAgent.velocity.sqrMagnitude < 0.1f;
    }
    
    public void SetAnimationState(CustomerAnimState state) 
    { 
        // TODO: 플레이어 애니메이터 연동
        Debug.Log($"Animation state changed to: {state}");
    }
    
    public void Despawn() 
    { 
        Debug.Log("Customer despawned");
        PoolManager.Instance.DespawnCustomer(this);
    }
    
    #endregion

    #region IPoolable

    /// <summary>
    /// 풀에서 가져온 후 데이터로 초기화 (ObjectPoolManager가 호출)
    /// </summary>
    public void InitializeFromPool(CustomerData customerData)
    {
        currentData = customerData;
        SetupCustomerData();
        SetupBT();
        
        if (showDebugInfo) Debug.Log($"[CustomerController]: 손님 데이터 초기화 완료 {customerData.customerName}");
    }
    
    public void OnGetFromPool()
    {
        if (showDebugInfo) Debug.Log("[CustomerController]: 풀에서 손님 데이터 가져옴");
    }

    public void OnReturnToPool()
    {
        if (showDebugInfo) Debug.Log("[CustomerController]: 풀으로 손님 데이터 반환");
        
        // BT 정리
        rootNode?.Reset();
        rootNode = null;
        
        // NavMeshAgent 정리
        if (navAgent && navAgent.isOnNavMesh)
        {
            navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
        }
        
        SetAnimationState(CustomerAnimState.Idle);
        
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void OnDestroyFromPool()
    {
        if (showDebugInfo) Debug.Log("[CustomerController]: 손님 데이터 풀에서 삭제");
    }

    #endregion
    
    #region public Getters
    
    public CustomerData CurrentData => currentData;
    
    #endregion
}