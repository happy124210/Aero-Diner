using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 생성 후 입구로 이동
/// </summary>
public class MoveToEntrance : BaseNode
{
    public override string NodeName => "MoveToEntrance";

    private bool hasStartedMoving = false;
    
    public MoveToEntrance(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        if (!hasStartedMoving)
        {
            // CustomerSpawner에서 입구 위치 가져오기
            Vector3 destination = CustomerSpawner.Instance.GetEntrancePosition();
            
            if (destination == Vector3.zero)
            {
                Debug.LogError($"Invalid entrance position for {NodeName}!");
                return NodeState.Failure;
            }
            
            Debug.Log($"Customer moving to entrance at {destination}");
            customer.SetDestination(destination);
            hasStartedMoving = true;
            
            return NodeState.Running;
        }
        
        // 목적지 도착 체크
        if (customer.HasReachedDestination())
        {
            customer.SetAnimationState(CustomerAnimState.Idle);
            Debug.Log($"Customer arrived at entrance");
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
/// 좌석으로 이동 - 이미 할당된 좌석으로 이동
/// </summary>
public class MoveToSeat : BaseNode
{
    public override string NodeName => "MoveToSeat";

    private bool hasStartedMoving = false;
    
    public MoveToSeat(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        if (!hasStartedMoving)
        {
            //이미 할당된 좌석 위치 가져오기
            Vector3 destination = customer.GetAssignedSeatPosition();
            
            if (destination == Vector3.zero)
            {
                Debug.LogError($"No seat assigned for customer! CheckAvailableSeat should run first.");
                return NodeState.Failure;
            }
            
            Debug.Log($"Customer moving to assigned seat at {destination}");
            customer.SetDestination(destination);
            hasStartedMoving = true;
            
            return NodeState.Running;
        }
        
        // 목적지 도착 체크
        if (customer.HasReachedDestination())
        {
            customer.SetAnimationState(CustomerAnimState.Idle);
            Debug.Log($"Customer arrived at assigned seat");
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
/// 빈 좌석이 있는지 체크 및 할당
/// </summary>
public class CheckAvailableSeat : BaseNode
{
    public override string NodeName => "CheckAvailableSeat";
    
    public CheckAvailableSeat(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        // CustomerSpawner를 통해 좌석 체크 및 할당
        bool hasAvailableSeat = customer.HasAvailableSeat(); 
        
        if (hasAvailableSeat)
        {
            Debug.Log("Available seat found and assigned!");
            return NodeState.Success;
        }
        
        Debug.Log("No available seats, customer will wait or leave...");
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
/// 이탈 - 출구로 이동하고 좌석 해제
/// </summary>
public class Leave : BaseNode
{
    public override string NodeName => "Leave";
    
    private enum State { Moving, Left }
    private State currentState = State.Moving;
    private bool hasStartedMoving = false;
    
    public Leave(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        switch (currentState)
        {
            case State.Moving:
                // 처음에만 목적지 설정
                if (!hasStartedMoving)
                {
                    // 자리 떠나자마자 해제 처리
                    Vector3 assignedSeat = customer.GetAssignedSeatPosition();
                    if (assignedSeat != Vector3.zero)
                    {
                        CustomerSpawner.Instance.ReleaseSeat(assignedSeat);
                        Debug.Log($"Released seat at {assignedSeat}");
                    }
                    
                    // CustomerSpawner에서 출구 위치 가져오기
                    Vector3 exitPosition = CustomerSpawner.Instance.GetExitPosition();
                    
                    if (exitPosition == Vector3.zero)
                    {
                        Debug.LogError("Invalid exit position!");
                        return NodeState.Failure;
                    }
                    
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
                customer.Despawn(); // 풀로 반환
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