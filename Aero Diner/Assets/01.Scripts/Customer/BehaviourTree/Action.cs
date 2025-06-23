using System.Collections;
using UnityEngine;

/// <summary>
/// 생성 후 입구로 이동
/// </summary>
public class MoveToEntrance : BaseNode
{
    public override string NodeName => "MoveToEntrance";
    
    private enum State { Moving, Arrived }
    private State currentState = State.Moving;
    
    public MoveToEntrance(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        switch (currentState)
        {
            case State.Moving:
                Debug.Log("Customer moving to entrance...");
                
                // TODO: 실제 이동 로직
                customer.StartCoroutine(SimulateMovement());
                currentState = State.Arrived;
                return NodeState.Running;
                
            case State.Arrived:
                Debug.Log("Customer arrived at entrance");
                return NodeState.Success;
        }
        
        return NodeState.Failure;
    }
    
    public override void Reset()
    {
        currentState = State.Moving;
    }

    #region TestCode

    private IEnumerator SimulateMovement()
    {
        yield return new WaitForSeconds(2f);
        currentState = State.Arrived;
    }

    #endregion
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
/// 자리로 이동
/// </summary>
public class MoveToSeat : BaseNode
{
    public override string NodeName => "MoveToSeat";
    
    private enum State { Moving, Arrived }
    private State currentState = State.Moving;
    
    public MoveToSeat(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        switch (currentState)
        {
            case State.Moving:
                Debug.Log("Customer moving to seat...");
                
                // TODO: 실제 좌석 이동 로직
                Vector3 seatPosition = customer.GetAssignedSeatPosition();
                customer.SetDestination(seatPosition);
                customer.StartCoroutine(SimulateMovement());
                currentState = State.Arrived;
                return NodeState.Running;
                
            case State.Arrived:
                if (customer.HasReachedDestination())
                {
                    Debug.Log("Customer arrived at seat");
                    customer.SetAnimationState(CustomerAnimState.Idle);
                    return NodeState.Success;
                }
                return NodeState.Running;
        }
        
        return NodeState.Failure;
    }
    
    public override void Reset()
    {
        currentState = State.Moving;
    }

    private IEnumerator SimulateMovement()
    {
        yield return new WaitForSeconds(2f);
        currentState = State.Arrived;
    }
}

/// <summary>
/// 주문받기
/// </summary>
public class TakeOrder : BaseNode
{
    public override string NodeName => "TakeOrder";
    
    private bool orderPlaced = false;
    
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
public class Leave : BaseNode
{
    public override string NodeName => "Leave";
    
    private enum State { Moving, Left }
    private State currentState = State.Moving;
    
    public Leave(CustomerController customer) : base(customer) { }
    
    public override NodeState Execute()
    {
        switch (currentState)
        {
            case State.Moving:
                Debug.Log("Customer leaving...");
                
                // TODO: 실제 퇴장 로직
                Vector3 exitPosition = customer.GetExitPosition();
                customer.SetDestination(exitPosition);
                customer.StartCoroutine(SimulateLeaving());
                currentState = State.Left;
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
    }
    
    private IEnumerator SimulateLeaving()
    {
        yield return new WaitForSeconds(2f);
        currentState = State.Left;
    }
}