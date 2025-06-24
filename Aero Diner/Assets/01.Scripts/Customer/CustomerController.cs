using System;
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

public class CustomerController : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] public string currentNodeName;
    
    [Header("Customer Stats")]
    [SerializeField] private CustomerData data;
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

    private void Start()
    {
        Debug.Log($"Customer position: {transform.position}");
        Debug.Log($"NavMesh exists: {NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas)}");
        
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

    private void SetupCustomerData()
    {
        speed = data.speed; 
        maxWaitTime = data.waitTime;
        eatTime = data.eatTime;
        
        currentPatience = maxWaitTime;
    }
    
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
        navAgent.autoBraking = true;
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
        // TODO: 실제 좌석 매니저와 연동
        // 임시로 랜덤 확률로 처리
        bool hasSeats = Random.Range(0f, 1f) > 0.3f;
        if (hasSeats)
        {
            // 좌석 할당
            assignedSeatPosition = seatPoint.position;
        }
        return hasSeats;
    }
    
    public Vector3 GetAssignedSeatPosition() => assignedSeatPosition;

    public Vector3 GetEntrancePosition() => entrancePoint.position; // 입구 위치
    public Vector3 GetExitPosition() => exitPoint.position; // 출구 위치
    
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
    
    public void SetDestination(Vector3 destination) 
    { 
        if (navAgent && navAgent.isOnNavMesh)
        {
            // NavMeshPlus Y축 버그 방지용
            if (Mathf.Abs(transform.position.x - destination.x) < 0.0001f)
            {
                destination.x += 0.0001f;
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
        Destroy(gameObject); // TODO: ObjectPool로 변경
    }
    
    #endregion
}