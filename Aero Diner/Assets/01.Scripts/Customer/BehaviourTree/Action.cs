using UnityEngine;

/// <summary>
/// 생성 후 입구로 이동
/// </summary>
public class MoveToEntrance : BaseNode
{
    public override string NodeName => "MoveToEntrance";

    private bool hasStartedMoving;
    
    public MoveToEntrance(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        if (!customer)
        {
            Debug.LogError($"[{NodeName}]: customerController 없음 !!!");
            return NodeState.Failure;
        }
        
        if (!hasStartedMoving)
        {
            Vector3 destination = CustomerSpawner.Instance.GetEntrancePosition();
            
            if (destination == Vector3.zero)
            {
                Debug.LogError($"[{NodeName}]: 입구 위치 없음 !!!");
                return NodeState.Failure;
            }
            
            customer.SetDestination(destination);
            hasStartedMoving = true;
            
            return NodeState.Running;
        }
        
        // 목적지 도착 체크
        if (customer.HasReachedDestination())
        {
            customer.SetAnimationState(CustomerAnimState.Idle);
            return NodeState.Success;
        }
        
        return NodeState.Running;
    }
    
    public override void Reset()
    {
        hasStartedMoving = false;
    }
}

/// <summary>
/// 줄서기 노드
/// </summary>
public class WaitInLine : BaseNode
{
    public override string NodeName => "WaitInLine";
    
    private enum State { JoiningQueue, WaitingInQueue, MovingInQueue }
    private State currentState = State.JoiningQueue;
    private float seatCheckTimer;
    private const float SEAT_CHECK_INTERVAL = 1f;
    
    public WaitInLine(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        if (!customer)
        {
            Debug.LogError($"[{NodeName}]: customer가 null입니다!");
            return NodeState.Failure;
        }
        
        // 인내심 체크
        if (!customer.HasPatience())
        {
            Debug.Log($"[{NodeName}]: 줄 기다리는 중에 인내심 소진됨");
            return NodeState.Failure;
        }
        
        switch (currentState)
        {
            case State.JoiningQueue:
                return HandleJoiningQueue();
                
            case State.WaitingInQueue:
                return HandleWaitingInQueue();
                
            case State.MovingInQueue:
                return HandleMovingInQueue();
        }
        
        return NodeState.Failure;
    }
    
    private NodeState HandleJoiningQueue()
    {
        if (!CustomerSpawner.Instance)
        {
            Debug.LogError($"[{NodeName}]: CustomerSpawner.Instance가 null입니다!");
            return NodeState.Failure;
        }
        
        // 줄에 합류 시도
        bool joinedQueue = CustomerSpawner.Instance.AddCustomerToQueue(customer);
        
        if (!joinedQueue)
        {
            Debug.LogWarning($"[{NodeName}]: 줄이 꽉 찼음");
            return NodeState.Failure;
        }
        
        Vector3 queuePosition = customer.GetCurrentQueuePosition();
        if (queuePosition == Vector3.zero)
        {
            Debug.LogError($"[{NodeName}]: 줄 위치를 얻지 못함!");
            return NodeState.Failure;
        }
        
        Debug.Log($"[{NodeName}]: 줄로 이동 중... {queuePosition}");
        customer.SetDestination(queuePosition);
        currentState = State.WaitingInQueue;
        
        return NodeState.Running;
    }
    
    /// <summary>
    /// 줄 기다리는 큐 담당
    /// </summary>
    private NodeState HandleWaitingInQueue()
    {
        // 줄 위치 도착 체크
        if (!customer.HasReachedDestination())
        {
            // 타이머 시작
            customer.StartPatienceTimer();
            return NodeState.Running;
        }
        
        // 줄에서 앞으로 이동해야 하는지 체크
        if (!customer.HasReachedQueuePosition())
        {
            currentState = State.MovingInQueue;
            return NodeState.Running;
        }
        
        customer.SetAnimationState(CustomerAnimState.Idle);
        
        // 주기적으로 좌석 확인 (줄의 맨 앞에 있을 때만)
        seatCheckTimer += Time.deltaTime;
        
        if (seatCheckTimer >= SEAT_CHECK_INTERVAL)
        {
            seatCheckTimer = 0f;

            if (!CustomerSpawner.Instance)
            {
                Debug.LogError($"[{NodeName}]: CustomerSpawner.Instance가 null입니다!");
                return NodeState.Failure;
            }
            
            // 내가 줄의 맨 앞에 있는지 확인
            var nextCustomer = CustomerSpawner.Instance.GetNextCustomerInQueue();
            if (nextCustomer == customer)
            {
                Debug.Log($"[{NodeName}]: 줄 맨 앞에서 좌석 확인 중");
                
                // 좌석 확인
                if (customer.HasAvailableSeat())
                {
                    customer.StopPatienceTimer();
                    Debug.Log($"[{NodeName}]: 줄에서 좌석 발견. 좌석으로 이동.");
                    
                    // 줄에서 제거
                    CustomerSpawner.Instance.RemoveCustomerFromQueue(customer);
                    
                    return NodeState.Success; // 좌석 찾음
                }
                
                Debug.Log($"[{NodeName}]: 아직 좌석 없음, 계속 대기");
            }
            else
            {
                Debug.Log($"[{NodeName}]: 줄에서 대기 중");
            }
        }
        
        return NodeState.Running; // 계속 기다리기
    }
    
    private NodeState HandleMovingInQueue()
    {
        // 새로운 줄 위치로 이동 완료 체크
        if (customer.HasReachedQueuePosition())
        {
            Debug.Log($"[{NodeName}]: 새로운 줄 위치 도착");
            currentState = State.WaitingInQueue;
            seatCheckTimer = 0f; // 좌석 체크 타이머 리셋
        }
        
        return NodeState.Running;
    }
    
    public override void Reset()
    {
        currentState = State.JoiningQueue;
        seatCheckTimer = 0f;
    }
}

/// <summary>
/// 할당된 좌석으로 이동
/// </summary>
public class MoveToSeat : BaseNode
{
    public override string NodeName => "MoveToSeat";

    private bool hasStartedMoving;
    
    public MoveToSeat(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        if (!customer)
        {
            Debug.LogError($"[{NodeName}]: customerController 없음!");
            return NodeState.Failure;
        }
        
        if (!hasStartedMoving)
        {
            //이미 할당된 좌석 위치 가져오기
            Vector3 destination = customer.GetAssignedSeatPosition();
            
            if (destination == Vector3.zero)
            {
                Debug.LogError($"[{NodeName}]: No seat assigned for customer! CheckAvailableSeat should run first.");
                return NodeState.Failure;
            }
            
            Debug.Log($"[{NodeName}]: Customer moving to assigned seat at {destination}");
            customer.SetDestination(destination);
            hasStartedMoving = true;
            
            return NodeState.Running;
        }
        
        // 목적지 도착 체크
        if (customer.HasReachedDestination())
        {
            customer.SetAnimationState(CustomerAnimState.Idle);
            Debug.Log($"[{NodeName}]: Customer arrived at assigned seat");
            return NodeState.Success;
        }
        
        return NodeState.Running;
    }
    
    public override void Reset()
    {
        hasStartedMoving = false;
    }
}

/// <summary>
/// 빈 좌석이 있는지 체크 및 할당
/// </summary>
public class CheckAvailableSeat : BaseNode
{
    public override string NodeName => "CheckAvailableSeat";
    
    public CheckAvailableSeat(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        if (!customer)
        {
            Debug.LogError($"[{NodeName}]: customer가 null입니다!");
            return NodeState.Failure;
        }

        if (!CustomerSpawner.Instance)
        {
            Debug.LogError($"[{NodeName}]: CustomerSpawner.Instance가 null입니다!");
            return NodeState.Failure;
        }
        
        // 좌석 할당 시도
        bool hasAvailableSeat = customer.HasAvailableSeat();
        
        if (hasAvailableSeat)
        {
            Debug.Log($"[{NodeName}]: 좌석 할당 성공");
            return NodeState.Success;
        }
        
        Debug.Log($"[{NodeName}]: 사용 가능한 좌석 없음");
        return NodeState.Failure;
    }
}

/// <summary>
/// 주문하기
/// </summary>
public class TakeOrder : BaseNode
{
    public override string NodeName => "TakeOrder";
    
    private bool orderPlaced;
    
    public TakeOrder(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        if (!customer)
        {
            Debug.LogError($"[{NodeName}]: customer가 null입니다!");
            return NodeState.Failure;
        }
        
        if (!orderPlaced)
        {
            Debug.Log($"[{NodeName}]: Customer placing order...");
            // TODO: 실제 주문 로직
            customer.PlaceOrder();
            orderPlaced = true;
            return NodeState.Running;
        }
        
        // 음식이 서빙되었는지 체크
        if (customer.IsFoodServed())
        {
            Debug.Log($"[{NodeName}]: Food served! Starting to eat...");
            customer.StopPatienceTimer();
            customer.StartEating();
            return NodeState.Success;
        }
        
        return NodeState.Running;
    }
    
    public override void Reset()
    {
        orderPlaced = false;
    }
}

/// <summary>
/// 결제
/// </summary>
public class Payment : BaseNode
{
    public override string NodeName => "Payment";
    
    private enum State { Eating, ProcessingPayment, PaymentDone }
    private State currentState = State.Eating;
    private const float PAYMENT_PROCESS_TIME = 1f;
    private float paymentTimer;
    
    public Payment(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        if (!customer)
        {
            Debug.LogError($"[{NodeName}]: customer가 null입니다!");
            return NodeState.Failure;
        }
        
        switch (currentState)
        {
            case State.Eating:
                if (customer.IsEatingFinished())
                {
                    Debug.Log($"[{NodeName}]: Customer finished eating, processing payment...");
                    customer.ProcessPayment();
                    currentState = State.ProcessingPayment;
                    paymentTimer = 0f;
                }

                break;
                
            case State.ProcessingPayment:
                // 결제 처리 시간 (타이머형식)
                paymentTimer += Time.deltaTime;
                if (paymentTimer >= PAYMENT_PROCESS_TIME)
                {
                    currentState = State.PaymentDone;
                }

                break;
                
            case State.PaymentDone:
                Debug.Log($"[{NodeName}]: Payment completed!");
                return NodeState.Success;
        }
        
        return NodeState.Running;
    }
    
    public override void Reset()
    {
        currentState = State.Eating;
        paymentTimer = 0f;
    }
}


/// <summary>
/// 출구로 이동하고 좌석 해제
/// </summary>
public class Leave : BaseNode
{
    public override string NodeName => "Leave";
    
    private enum State { PreparingToLeave, Moving, Left }
    private State currentState = State.PreparingToLeave;
    private bool hasStartedMoving;
    private bool seatReleased;
    
    public Leave(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        if (!customer)
        {
            Debug.LogError($"[{NodeName}]: customer가 null입니다!");
            return NodeState.Failure;
        }
        
        switch (currentState)
        {
            case State.PreparingToLeave:
                return HandlePreparingToLeave();
                
            case State.Moving:
                return HandleMoving();
                
            case State.Left:
                return HandleLeft();
        }
        
        return NodeState.Failure;
    }
    
    /// <summary>
    /// 떠나기 준비
    /// </summary>
    private NodeState HandlePreparingToLeave()
    {
        // 좌석 해제
        if (!seatReleased)
        {
            Vector3 assignedSeat = customer.GetAssignedSeatPosition();
            if (assignedSeat != Vector3.zero && CustomerSpawner.Instance)
            {
                CustomerSpawner.Instance.ReleaseSeat(assignedSeat);
                Debug.Log($"[{NodeName}]: Released seat at {assignedSeat}");
            }
            seatReleased = true;
        }
        
        currentState = State.Moving;
        return NodeState.Running;
    }
    
    /// <summary>
    /// 출구로 이동
    /// </summary>
    private NodeState HandleMoving()
    {
        // 처음에만 목적지 설정
        if (!hasStartedMoving)
        {
            if (!CustomerSpawner.Instance)
            {
                Debug.LogError($"[{NodeName}]: CustomerSpawner.Instance가 null입니다!");
                return NodeState.Failure;
            }
            
            // CustomerSpawner에서 출구 위치 가져오기
            Vector3 exitPosition = CustomerSpawner.Instance.GetExitPosition();
            
            if (exitPosition == Vector3.zero)
            {
                Debug.LogError($"[{NodeName}]: Invalid exit position!");
                return NodeState.Failure;
            }
            
            Debug.Log($"[{NodeName}]: Customer starting to leave to {exitPosition}");
            customer.SetDestination(exitPosition);
            hasStartedMoving = true;
        }
        
        // 도착 체크
        if (customer.HasReachedDestination())
        {
            Debug.Log($"[{NodeName}]: Customer reached exit");
            currentState = State.Left;
        }
        
        return NodeState.Running;
    }
    
    /// <summary>
    /// 떠남 처리
    /// </summary>
    private NodeState HandleLeft()
    {
        Debug.Log($"[{NodeName}]: Customer left the restaurant");
        customer.Despawn();
        return NodeState.Success;
    }
    
    public override void Reset()
    {
        currentState = State.PreparingToLeave;
        hasStartedMoving = false;
        seatReleased = false;
    }
}