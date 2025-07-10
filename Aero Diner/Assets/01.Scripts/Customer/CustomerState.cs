using UnityEngine;

public enum CustomerStateName
{
    MovingToEntrance,
    MovingToLine,
    WaitingInLine,
    MovingToSeat,
    Ordering,
    Eating,
    Paying,
    Leaving
}

public abstract class CustomerState
{
    public abstract void Enter(CustomerController customer);
    public abstract CustomerState Update(CustomerController customer);
    public abstract void Exit(CustomerController customer);
    public abstract CustomerStateName Name { get; }
}

/// <summary>
/// 입구로 이동
/// </summary>
public class MovingToEntranceState : CustomerState
{
    public override CustomerStateName Name => CustomerStateName.MovingToEntrance;
    
    public override void Enter(CustomerController customer)
    {
        Vector3 entrance = RestaurantManager.Instance.GetEntrancePoint();
        customer.SetDestination(entrance);
        customer.SetAnimationState(CustomerAnimState.Walking);
    }
    
    public override CustomerState Update(CustomerController customer)
    {
        // 좌석 할당 시도
        if (TableManager.Instance.TryAssignSeat(customer))
        {
            // 바로 좌석 할당됨
            if (customer.GetAssignedTable())
            {
                return new MovingToSeatState();
            }
                
            // 줄 서기
            return new MovingToLineState();
        }
        // 레스토랑 꽉 참
        return new LeavingState();
    }
    
    public override void Exit(CustomerController customer) { }
}

/// <summary>
/// 계산된 줄서기 위치로 이동
/// </summary>
public class MovingToLineState : CustomerState
{
    public override CustomerStateName Name => CustomerStateName.MovingToLine;
    
    public override void Enter(CustomerController customer)
    {
        Vector3 queuePosition = TableManager.Instance.GetCustomerQueuePosition(customer);
        customer.SetDestination(queuePosition);
        customer.SetAnimationState(CustomerAnimState.Walking);
    }

    public override CustomerState Update(CustomerController customer)
    {
        if (customer.HasReachedDestination())
        {
            return new WaitingInLineState();
        }
        
        if (!customer.HasPatience())
        {
            return new LeavingState();
        }

        return this;
    }

    public override void Exit(CustomerController customer) 
    {
        customer.StopMovement();
        customer.SetAnimationState(CustomerAnimState.Idle);
    }
}


/// <summary>
/// 줄 서서 대기
/// </summary>
public class WaitingInLineState : CustomerState
{
    public override CustomerStateName Name => CustomerStateName.WaitingInLine;

    public override void Enter(CustomerController customer) 
    {
        customer.SetPatienceTimerActive(true);
    }

    public override CustomerState Update(CustomerController customer)
    {
        if (!customer.HasPatience())
        {
            return new LeavingState();
        }

        return this;
    }

    public override void Exit(CustomerController customer) 
    {
        customer.SetPatienceTimerActive(false);
    }
}


/// <summary>
/// 할당된 좌석으로 이동
/// </summary>
public class MovingToSeatState : CustomerState
{
    public override CustomerStateName Name => CustomerStateName.MovingToSeat;
    
    public override void Enter(CustomerController customer)
    {
        customer.SetDestination(customer.GetStopPosition());
        customer.SetAnimationState(CustomerAnimState.Walking);
    }
    
    public override CustomerState Update(CustomerController customer)
    {
        if (customer.HasReachedDestination())
        {
            return new OrderingState();
        }
        
        return this;
    }

    public override void Exit(CustomerController customer)
    {
        customer.StopMovement();
        customer.SetAnimationState(CustomerAnimState.Sitting);
    }
}

/// <summary>
/// 주문하기
/// </summary>
public class OrderingState : CustomerState
{
    public override CustomerStateName Name => CustomerStateName.Ordering;
    
    public override void Enter(CustomerController customer) 
    {
        customer.AdjustSeatPosition();
        customer.PlaceOrder();
        customer.SetPatienceTimerActive(true);
    }
    
    public override CustomerState Update(CustomerController customer)
    {
        if (!customer.HasPatience())
        {
            return new LeavingState();
        }
            
        return this;
    }
    
    public override void Exit(CustomerController customer) 
    {
        customer.SetPatienceTimerActive(false);
        EventBus.Raise(UIEventType.HideOrderPanel, customer);
    }
}

/// <summary>
/// 식사하기
/// </summary>
public class EatingState : CustomerState
{
    public override CustomerStateName Name => CustomerStateName.Eating;
    private float eatTimer;
    
    public override void Enter(CustomerController customer)
    {
        customer.GetAssignedTable().GetCurrentFood().isPickupable = false;
        eatTimer = customer.GetEatingTime();
        customer.SetAnimationState(CustomerAnimState.Sitting);
    }
    
    public override CustomerState Update(CustomerController customer)
    {
        eatTimer -= Time.deltaTime;
        if (eatTimer <= 0)
        {
            return new PayingState();
        }
            
        return this;
    }
    
    public override void Exit(CustomerController customer) 
    {
        customer.SetAnimationState(CustomerAnimState.Sitting);
    }
}

/// <summary>
/// 결제하기
/// </summary>
public class PayingState : CustomerState
{
    public override CustomerStateName Name => CustomerStateName.Paying;
    
    private const float PAYMENT_TIME = 1f;
    private float paymentTimer;
    
    public override void Enter(CustomerController customer)
    {
        paymentTimer = 0;
        customer.ProcessPayment();
    }
    
    public override CustomerState Update(CustomerController customer)
    {
        paymentTimer += Time.deltaTime;
        if (paymentTimer >= PAYMENT_TIME)
        {
            return new LeavingState();
        }
            
        return this;
    }

    public override void Exit(CustomerController customer) { }
}

/// <summary>
/// 자리 떠나기
/// </summary>
public class LeavingState : CustomerState
{
    public override CustomerStateName Name => CustomerStateName.Leaving;
    
    public override void Enter(CustomerController customer)
    {
        Vector3 exit = RestaurantManager.Instance.GetExitPoint();
        customer.SetDestination(exit);
        customer.SetAnimationState(CustomerAnimState.Walking);
        
        TableManager.Instance.ReleaseSeat(customer);
        TableManager.Instance.RemoveCustomerFromQueue(customer);
    }
    
    public override CustomerState Update(CustomerController customer)
    {
        if (customer.HasReachedDestination())
        {
            customer.Despawn();
            return null;
        }
        return this;
    }
    
    public override void Exit(CustomerController customer) { }
}