using System;
using UnityEngine;

/// <summary>
/// 고객의 모든 데이터를 관리하는 Model
/// View나 Controller에 대한 정보는 알지 못함
/// </summary>
public class Customer
{
    // 이벤트 정의
    public event Action<float> OnPatienceChanged; // 인내심 줄어들고 있음
    public event Action<bool> OnPatienceStateChanged; // 인내심 줄어들어야 하는지 아닌지
    public event Action<FoodData> OnOrderPlaced; // 주문함
    public event Action<bool> OnServedStateChanged; // 서빙됨
    public event Action<bool> OnEatingStateChanged; // 다 먹음
    public event Action<bool> OnPaymentStateChanged; // 결제 끝남
    
    public event Action<CustomerAnimState> OnAnimationStateChanged;
    
    // 고객 기본 데이터
    private CustomerData customerData;
    private float speed;
    private float maxPatience;
    private float eatTime;

    // 현재 상태 데이터
    private float currentPatience;
    private bool isPatienceDecreasing;
    private FoodData currentOrder;
    private Table assignedTable;

    // 상태 플래그들
    private bool isServed;
    private bool isEating;
    private bool isPaymentCompleted;
    private bool hasLeftRestaurant;

    // 타이머
    private float eatingTimer;

    #region Properties
    public CustomerData CustomerData => customerData;
    public float Speed => speed;
    public float MaxPatience => maxPatience;
    public float EatTime => eatTime;
    public float CurrentPatience => currentPatience;
    public bool IsPatienceDecreasing => isPatienceDecreasing;
    public FoodData CurrentOrder => currentOrder;
    public Table AssignedTable => assignedTable;
    public bool IsServed => isServed;
    public bool IsEating => isEating;
    public bool IsPaymentCompleted => isPaymentCompleted;
    public bool HasLeftRestaurant => hasLeftRestaurant;
    public bool HasPatience => currentPatience > 0;
    public float EatingTimer => eatingTimer;
    #endregion

    #region Initialization
    public void Initialize(CustomerData data)
    {
        customerData = data;
        speed = data.speed;
        maxPatience = data.waitTime;
        eatTime = data.eatTime;
        
        ResetData();
    }

    public void ResetData()
    {
        currentPatience = maxPatience;
        isPatienceDecreasing = false;
        currentOrder = null;
        assignedTable = null;
        
        isServed = false;
        isEating = false;
        isPaymentCompleted = false;
        hasLeftRestaurant = false;
        eatingTimer = 0f;

        // 초기화 이벤트
        OnPatienceChanged?.Invoke(currentPatience);
        OnPatienceStateChanged?.Invoke(isPatienceDecreasing);
        OnServedStateChanged?.Invoke(isServed);
        OnEatingStateChanged?.Invoke(isEating);
        OnPaymentStateChanged?.Invoke(isPaymentCompleted);
    }
    #endregion

    #region Data Updates
    public void UpdatePatience(float deltaTime)
    {
        if (!isPatienceDecreasing) return;

        currentPatience -= deltaTime;
        currentPatience = Mathf.Max(0, currentPatience);
        OnPatienceChanged?.Invoke(currentPatience);
    }

    public void UpdateEatingTimer(float deltaTime)
    {
        if (!isEating) return;

        eatingTimer += deltaTime;
        if (eatingTimer >= eatTime)
        {
            isEating = false;
            OnEatingStateChanged?.Invoke(isEating);
        }
    }

    public void StartPatienceTimer()
    {
        isPatienceDecreasing = true;
        OnPatienceStateChanged?.Invoke(isPatienceDecreasing);
    }

    public void StopPatienceTimer()
    {
        isPatienceDecreasing = false;
        currentPatience = maxPatience;
        OnPatienceStateChanged?.Invoke(isPatienceDecreasing);
        OnPatienceChanged?.Invoke(currentPatience);
    }

    public void SetAssignedTable(Table table)
    {
        assignedTable = table;
    }

    public void PlaceOrder()
    {
        currentOrder = MenuManager.Instance.GetRandomMenu();
        OnOrderPlaced?.Invoke(currentOrder);
    }

    public void ReceiveFood(FoodData servedMenu)
    {
        if (isServed || currentOrder == null) return;

        if (currentOrder.id == servedMenu.id)
        {
            isServed = true;
            OnServedStateChanged?.Invoke(isServed);
            MenuManager.Instance.OnMenuServed(servedMenu.id);
        }
    }

    public void StartEating()
    {
        isEating = true;
        eatingTimer = 0f;
        OnEatingStateChanged?.Invoke(isEating);
    }

    public void ProcessPayment()
    {
        if (!currentOrder) return;

        int payment = Mathf.RoundToInt(currentOrder.foodCost);
        RestaurantManager.Instance.OnCustomerPaid(payment);
        
        isPaymentCompleted = true;
        OnPaymentStateChanged?.Invoke(isPaymentCompleted);
    }

    public void SetLeftRestaurant()
    {
        hasLeftRestaurant = true;
    }
    #endregion
}