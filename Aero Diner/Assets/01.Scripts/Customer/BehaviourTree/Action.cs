using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 생성 후 입구로 이동
/// </summary>
public class MoveToPosition : BaseNode
{
    public override string NodeName { get; }

    private readonly Func<Vector3> getDestination;
    private bool hasStartedMoving = false;
    
    /// <summary>
    /// MoveToPosition 생성자
    /// </summary>
    /// <param name="customer">손님 컨트롤러</param>
    /// <param name="getDestination">목적지를 가져오는 함수</param>
    /// <param name="nodeName">노드 이름</param>
    public MoveToPosition(CustomerController customer, Func<Vector3> getDestination, 
        string nodeName = "MoveToPosition")
        : base(customer)
    {
        this.getDestination = getDestination;
        NodeName = nodeName;
    }
    
    public override NodeState Execute()
    {
        if (!hasStartedMoving)
        {
            Vector3 destination = getDestination();
            
            if (destination == Vector3.zero)
            {
                Debug.LogError($"Invalid destination for {NodeName}!");
                return NodeState.Failure;
            }
            
            Debug.Log($"Customer moving to {destination}");
            customer.SetDestination(destination);
            hasStartedMoving = true;
            
            return NodeState.Running;
        }
        
        // 목적지 도착 체크
        if (customer.HasReachedDestination())
        {
            customer.SetAnimationState(CustomerAnimState.Idle);
            Debug.Log($"Customer arrived at destination ({NodeName})");
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
/// 대기시간 잔여시간 체크
/// </summary>
public class CheckWaitingTime : BaseNode
{
    public override string NodeName => "CheckWaitingTime";
    
    public CheckWaitingTime(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        // 결제 완료 후에는 인내심 체크 생략
        if (customer.IsPaymentCompleted())
        {
            Debug.Log("Payment completed, skipping patience check");
            return NodeState.Success;
        }
        
        // TODO: 실제 인내심 시간 체크 로직
        float remainingTime = customer.GetRemainingPatience();
        
        if (remainingTime <= 0)
        {
            Debug.Log("Customer out of patience!");
            return NodeState.Failure;
        }
        
        Debug.Log($"Customer patience remaining: {remainingTime}s");
        return NodeState.Success;
    }
}

/// <summary>
/// 빈 좌석이 있는지 체크
/// </summary>
public class CheckAvailableSeat : BaseNode
{
    public override string NodeName => "CheckAvailableSeat";
    
    public CheckAvailableSeat(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        // TODO: 실제 좌석 체크 로직
        bool hasAvailableSeat = customer.HasAvailableSeat();
        
        if (hasAvailableSeat)
        {
            Debug.Log("Available seat found!");
            return NodeState.Success;
        }
        
        Debug.Log("No available seats, waiting in line...");
        return NodeState.Failure;
    }
}


/// <summary>
/// 주문받기
/// </summary>
public class TakeOrder : BaseNode
{
    public override string NodeName => "TakeOrder";
    
    private bool orderPlaced;
    
    public TakeOrder(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        if (!orderPlaced)
        {
            Debug.Log("Customer placing order...");
            // TODO: 실제 주문 로직
            customer.PlaceOrder();
            orderPlaced = true;
            return NodeState.Running;
        }
        
        // 음식이 서빙되었는지 체크
        if (customer.IsFoodServed())
        {
            Debug.Log("Food served! Starting to eat...");
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
    
    private enum State { Eating, PaymentDone }
    private State currentState = State.Eating;
    
    public Payment(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        switch (currentState)
        {
            case State.Eating:
                if (customer.IsEatingFinished())
                {
                    Debug.Log("Customer finished eating, processing payment...");
                    customer.ProcessPayment();
                    customer.StartCoroutine(PaymentDelay());
                    currentState = State.PaymentDone;
                }
                return NodeState.Running;
                
            case State.PaymentDone:
                Debug.Log("Payment completed!");
                return NodeState.Success;
        }
        
        return NodeState.Running;
    }
    
    public override void Reset()
    {
        currentState = State.Eating;
    }
    
    private IEnumerator PaymentDelay()
    {
        yield return new WaitForSeconds(1f); // 결제 연출 시간
        currentState = State.PaymentDone;
    }
}

/// <summary>
/// 이탈
/// </summary>
public class LeaveRestaurant : BaseNode
{
    public override string NodeName => "LeaveRestaurant";
    
    private enum State { Moving, Left }
    private State currentState = State.Moving;
    private bool hasStartedMoving = false;
    
    public LeaveRestaurant(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        switch (currentState)
        {
            case State.Moving:
                // 처음에만 목적지 설정
                if (!hasStartedMoving)
                {
                    Vector3 exitPosition = customer.GetExitPosition();
                    Debug.Log($"Customer starting to leave to {exitPosition}");
                    customer.SetDestination(exitPosition);
                    hasStartedMoving = true;
                }
                
                // 도착 체크
                if (customer.HasReachedDestination())
                {
                    Debug.Log("Customer reached exit");
                    currentState = State.Left;
                }
                
                return NodeState.Running;
                
            case State.Left:
                Debug.Log("Customer left the restaurant");
                customer.Despawn();
                return NodeState.Success;
        }
        
        return NodeState.Failure;
    }
    
    public override void Reset()
    {
        currentState = State.Moving;
        hasStartedMoving = false;
    }
}