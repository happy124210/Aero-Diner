using UnityEngine;

/// <summary>
/// 입구로 이동
/// </summary>
public class MovingToEntranceState : CustomerState
{
    public override string StateName => "MovingToEntrance";
    
    public override void Enter(CustomerController customer)
    {
        Vector3 entrance = CustomerSpawner.Instance.GetEntrancePosition();
        customer.SetDestination(entrance);
    }
    
    public override CustomerState Update(CustomerController customer)
    {
        if (customer.HasReachedDestination())
        {
            // 좌석 있으면 바로 이동, 없으면 줄서기
            return TableManager.Instance.HasAvailableSeat() 
                ? new MovingToSeatState() 
                : new WaitingInLineState();
        }
        return this;
    }
    
    public override void Exit(CustomerController customer) { }
}

/// <summary>
/// 줄 서기
/// </summary>
public class WaitingInLineState : CustomerState
{
    public override string StateName => "WaitingInLine";
    
    private const float SEAT_CHECK_INTERVAL = 0.5f;
    private float seatCheckTimer;
    
    public override void Enter(CustomerController customer)
    {
        // 줄에 합류 후 타이머 시작
        CustomerSpawner.Instance.AddCustomerToQueue(customer);
        customer.StartPatienceTimer();
    }
    
    public override CustomerState Update(CustomerController customer)
    {
        // 인내심 소진 체크
        if (!customer.HasPatience())
            return new LeavingState();
            
        // 주기적으로 좌석 체크
        seatCheckTimer += Time.deltaTime;
        if (seatCheckTimer >= SEAT_CHECK_INTERVAL)
        {
            seatCheckTimer = 0f;
            
            // 내가 줄 맨 앞이고 좌석 있으면 이동
            if (CustomerSpawner.Instance.GetNextCustomerInQueue() == customer 
                && TableManager.Instance.HasAvailableSeat())
            {
                return new MovingToSeatState();
            }
        }
        
        return this;
    }
    
    public override void Exit(CustomerController customer)
    {
        CustomerSpawner.Instance.RemoveCustomerFromQueue(customer);
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
        Vector3 seatPos = customer.GetAssignedSeatPosition();
        customer.SetDestination(seatPos);
    }
    
    public override CustomerState Update(CustomerController customer)
    {
        if (customer.HasReachedDestination())
            return new OrderingState();
            
        return this;
    }

    public override void Exit(CustomerController customer)
    {
    }
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
            return new EatingState();
            
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
    }
    
    public override CustomerState Update(CustomerController customer)
    {
        if (customer.IsEatingFinished())
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

    public override void Exit(CustomerController customer)
    {
    }
}

/// <summary>
/// 자리 떠나기
/// </summary>
public class LeavingState : CustomerState
{
    public override string StateName => "Leaving";
    
    public override void Enter(CustomerController customer)
    {
        Vector3 exit = CustomerSpawner.Instance.GetExitPosition();
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