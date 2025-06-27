using System;
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
    
    [Header("Customer UI")]
    [SerializeField] private Canvas customerUI;
    [SerializeField] private Image orderBubble;
    [SerializeField] private Image patienceTimer;
    private bool isPatienceDecreasing;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;
    [SerializeField] public string currentNodeName;
    
    // 큐 관리
    private Vector3 currentQueuePosition = Vector3.zero;
    private bool isMovingToNewQueuePosition;
    
    // 상태 체크용 bool변수들
    private bool foodServed;
    private bool isEating;
    private bool eatingFinished;
    private bool paymentCompleted;
    private bool hasLeftRestaurant;

    // components
    private NavMeshAgent navAgent;
    
    private INode rootNode;
    private Vector3 assignedSeatPosition;
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
        SetupBT();
    }

    private void Update()
    {
        if (hasLeftRestaurant) return;
        
        UpdateCustomerUI();
        
        // 결제 완료 전까지만 인내심 감소
        if (isPatienceDecreasing && currentPatience > 0 && !paymentCompleted)
        {
            currentPatience -= Time.deltaTime;
            
            // 인내심이 0 이하가 되면 즉시 BT 리셋 - 이탈
            if (currentPatience <= 0)
            {
                HideAllUI();
                if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 인내심 소진!");
                rootNode?.Reset();
                return;
            }
        }
        
        // 식사 중 타이머 처리
        if (isEating)
        {
            eatingTimer += Time.deltaTime;
            if (eatingTimer >= eatTime)
            {
                isEating = false;
                eatingFinished = true;
                if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 식사 완료!");
            }
        }
        
        // BT 실행
        if (rootNode != null)
        {
            NodeState state = rootNode.Execute();

            if (state == NodeState.Success || state == NodeState.Failure)
            {
                if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} BT completed with state: {state}");
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
        foodServed = false;
        isEating = false;
        eatingFinished = false;
        paymentCompleted = false;
        hasLeftRestaurant = false;
        eatingTimer = 0f;
        assignedSeatPosition = Vector3.zero;
        
        // 큐 데이터 초기화
        currentQueuePosition = Vector3.zero;
        isMovingToNewQueuePosition = false;
        
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
    
    /// <summary>
    /// 줄서기 우선 로직
    /// </summary>
    private void SetupBT()
    {
        // 좌석 시도 플로우
        var tryGetSeatFlow = new Selector(this,
            // 바로 좌석 있으면 성공
            new CheckAvailableSeat(this),
            // 좌석 없으면 줄서기
            new WaitInLine(this)
        );
    
        // 전체 손님 플로우
        var mainFlow = new Sequence(this,
            new MoveToEntrance(this),
            tryGetSeatFlow, // 좌석 확보
            new MoveToSeat(this),
            new Selector(this,
                new Sequence(this,
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
    
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} BT 셋업 완료");
    }

    public void SetCurrentNodeName(string newNodeName)
    {
        currentNodeName = newNodeName;
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} Current node: {currentNodeName}");
    }
    
#endregion
    
#region Customer Actions & State
    
    public float GetRemainingPatience() => currentPatience;
    
    /// <summary>
    /// 인내심이 있는지 체크하는 메서드
    /// </summary>
    public bool HasPatience() => currentPatience > 0;
    
    public bool HasAvailableSeat()
    {
        // 임시로 좌석 체크
        HideAllUI();
        return CustomerSpawner.Instance.AssignSeatToCustomer(this);
    }
    
    /// <summary>
    /// CustomerSpawner에서 할당된 좌석 위치 설정
    /// </summary>
    public void SetAssignedSeatPosition(Vector3 seatPosition)
    {
        assignedSeatPosition = seatPosition;
    
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 좌석 할당됨 {seatPosition}");
    }
    
    public Vector3 GetAssignedSeatPosition() => assignedSeatPosition;
    
    /// <summary>
    /// 줄 위치 업데이트 (CustomerSpawner가 호출)
    /// </summary>
    public void UpdateQueuePosition(Vector3 newQueuePosition)
    {
        currentQueuePosition = newQueuePosition;
        isMovingToNewQueuePosition = true;
        
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 새로운 줄 위치로 이동: {newQueuePosition}");
        SetDestination(newQueuePosition);
    }
    
    /// <summary>
    /// 현재 줄 위치 반환
    /// </summary>
    public Vector3 GetCurrentQueuePosition()
    {
        return currentQueuePosition;
    }
    
    /// <summary>
    /// 줄 위치 이동 완료 체크
    /// </summary>
    public bool HasReachedQueuePosition()
    {
        if (!isMovingToNewQueuePosition) return true;
        
        if (HasReachedDestination())
        {
            isMovingToNewQueuePosition = false;
            if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 새로운 줄 위치에 도착");
            return true;
        }
        
        return false;
    }
    
    public void PlaceOrder()
    {
        // TODO: 실제 주문 시스템과 연동
        ShowOrderBubble();
        StartPatienceTimer();
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 주문 완료!");
        
        // 임시로 2-5초 후 음식 서빙
        Invoke(nameof(ServeFood), Random.Range(2f, 5f));
    }
    
    private void ServeFood()
    {
        foodServed = true;
        StopPatienceTimer();
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 음식 서빙됨!");
    }
    
    public bool IsFoodServed() => foodServed;
    
    public void StartEating()
    {
        isEating = true;
        eatingTimer = 0f;
        eatingFinished = false;
        SetAnimationState(CustomerAnimState.Idle);
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 식사 시작");
    }
    
    public bool IsEatingFinished() => eatingFinished;
    public bool IsPaymentCompleted() => paymentCompleted;
    
    public void ProcessPayment()
    {
        // TODO: 실제 결제 시스템과 연동
        int payment = Random.Range(100, 500);
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} {payment} 코인 결제!");
        
        // 결제 완료 표시 (더 이상 인내심 감소 안함)
        paymentCompleted = true;
        
        // TODO: 결제 이펙트
    }
    
#endregion
   
#region Public Timer Control

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

#region Movement & Animation

    private const float AGENT_DRIFT = 0.0001f;
    
    /// <summary>
    /// 목적지 설정
    /// </summary>
    public void SetDestination(Vector3 destination) 
    { 
        if (!navAgent)
        {
            Debug.LogError($"[CustomerController]: {gameObject.name} NavMeshAgent가 null입니다!");
            return;
        }
        
        if (!navAgent.isOnNavMesh)
        {
            Debug.LogWarning($"[CustomerController]: {gameObject.name}이 NavMesh 위에 있지 않습니다!");
            return;
        }
        
        // NavMeshPlus Y축 버그 방지용
        if (Mathf.Abs(transform.position.x - destination.x) < AGENT_DRIFT)
        {
            destination.x += AGENT_DRIFT;
        }
        
        navAgent.SetDestination(destination);
        SetAnimationState(CustomerAnimState.Walking);
        
        if (showDebugInfo) 
            Debug.Log($"[CustomerController]: {gameObject.name} 목적지 설정: {destination}");
    }
    
    /// <summary>
    /// 목적지 도달 체크
    /// </summary>
    public bool HasReachedDestination()
    {
        const float ARRIVAL_THRESHOLD = 0.5f;
        const float VELOCITY_THRESHOLD = 0.1f;
        
        if (!navAgent || !navAgent.isOnNavMesh) 
        {
            if (showDebugInfo) Debug.LogWarning($"[CustomerController]: {gameObject.name} NavMeshAgent 문제!");
            return false;
        }
        
        bool reached = !navAgent.pathPending && 
                      navAgent.remainingDistance < ARRIVAL_THRESHOLD && 
                      navAgent.velocity.sqrMagnitude < VELOCITY_THRESHOLD;
        
        if (reached && showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 목적지 도착!");
            
        return reached;
    }
    
    public void SetAnimationState(CustomerAnimState state) 
    { 
        // TODO: 실제 애니메이터 연동
        if (showDebugInfo) Debug.Log($"[CustomerController]: {gameObject.name} 애니메이션 상태: {state}");
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
        SetupBT();
        
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
        rootNode?.Reset();
        rootNode = null;
        
        // NavMeshAgent 정리
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
        }
        
        // Queue 시스템에서 제거
        if (CustomerSpawner.Instance != null)
        {
            CustomerSpawner.Instance.RemoveCustomerFromQueue(this);
        }
        
        // 할당받은 좌석이 있다면 좌석 해제
        if (assignedSeatPosition != Vector3.zero && CustomerSpawner.Instance != null)
        {
            CustomerSpawner.Instance.ReleaseSeat(assignedSeatPosition);
        }
        
        // 큐 데이터 정리
        currentQueuePosition = Vector3.zero;
        isMovingToNewQueuePosition = false;
        
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
    
    #endregion
    
}