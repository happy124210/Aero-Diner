using UnityEngine;

/// <summary>
/// 입구로 이동
/// </summary>
public class MovingToEntranceState : CustomerState
{
    public override string StateName => "MovingToEntrance";
    
    public override void Enter(CustomerController customer)
    {
        Vector3 entrance = RestaurantManager.Instance.GetEntrancePoint();
        customer.SetDestination(entrance);
    }
    
    public override CustomerState Update(CustomerController customer)
    {
        if (TableManager.Instance.TryAssignSeat(customer))
        {
            // 좌석 할당 성공 시
            if (customer.GetAssignedTable())
            {
                // 바로 좌석으로 이동
                return new MovingToSeatState();
            }
            
            // 없으면 줄 서기 상태
            return new WaitingInLineState();
        }
        
        return new LeavingState();
    }
    
    public override void Exit(CustomerController customer) { }
}

/// <summary>
/// 줄 서기
/// </summary>
public class WaitingInLineState : CustomerState
{
    public override string StateName => "WaitingInLine";

    public override void Enter(CustomerController customer) { }
    
    public override CustomerState Update(CustomerController customer)
    {
        if (customer.HasReachedDestination())
            customer.StartPatienceTimer();
        
        if (!customer.HasPatience())
            return new LeavingState();
        
        return this;
    }

    public override void Exit(CustomerController customer)
    {
        customer.StopPatienceTimer();
    }
}

/// <summary>
/// 할당된 좌석으로 이동
/// </summary>
public class MovingToSeatState : CustomerState
{
    public override string StateName => "MovingToSeat";
    
    public override void Enter(CustomerController customer)
    {
        Vector3 seatPos = customer.GetAssignedStopPosition();
        customer.SetDestination(seatPos);
    }
    
    public override CustomerState Update(CustomerController customer)
    {
        if (customer.HasReachedDestination())
        {
            customer.AdjustSeatPosition();
            return new OrderingState();
        }
        
        return this;
    }

    public override void Exit(CustomerController customer) { }
}

/// <summary>
/// 주문하기
/// </summary>
public class OrderingState : CustomerState
{
    public override string StateName => "Ordering";
    private bool orderPlaced;
    
    public override void Enter(CustomerController customer) 
    {
        customer.StopMovement();
    }
    
    public override CustomerState Update(CustomerController customer)
    {
        // 인내심 소진 체크
        if (!customer.HasPatience())
            return new LeavingState();
            
        if (!orderPlaced)
        {
            customer.PlaceOrder();
            orderPlaced = true;
            customer.StartPatienceTimer();
        }

        if (customer.IsFoodServed())
        {
            return new EatingState();
        }
            
        return this;
    }
    
    public override void Exit(CustomerController customer)
    {
        customer.StopPatienceTimer();
    }
}

/// <summary>
/// 먹기
/// </summary>
public class EatingState : CustomerState
{
    public override string StateName => "Eating";
    
    public override void Enter(CustomerController customer)
    {
        customer.StartEating();

        Table table = customer.GetAssignedTable();
        table.GetCurrentFood().isPickupable = false;
    }
    
    public override CustomerState Update(CustomerController customer)
    {
        if (customer.IsCustomerEating())
            return new PayingState();
            
        return this;
    }
    
    public override void Exit(CustomerController customer) { }
}

/// <summary>
/// 결제하기
/// </summary>
public class PayingState : CustomerState
{
    public override string StateName => "Paying";
    
    // TODO: 임시 시간! 나중에 연출 시간이랑 맞춰야함
    private const float PAYMENT_TIME = 1f;
    private float paymentTimer;
    
    public override void Enter(CustomerController customer)
    {
        customer.ProcessPayment();
    }
    
    public override CustomerState Update(CustomerController customer)
    {
        paymentTimer += Time.deltaTime;
        if (paymentTimer >= PAYMENT_TIME)
            return new LeavingState();
            
        return this;
    }

    public override void Exit(CustomerController customer) { }
}

/// <summary>
/// 자리 떠나기
/// </summary>
public class LeavingState : CustomerState
{
    public override string StateName => "Leaving";
    
    public override void Enter(CustomerController customer)
    {
        Vector3 exit = RestaurantManager.Instance.GetExitPoint();
        customer.SetDestination(exit);
        TableManager.Instance.ReleaseSeat(customer);
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