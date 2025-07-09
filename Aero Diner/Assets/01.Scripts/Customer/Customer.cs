using System;
using UnityEngine;

/// <summary>
/// 고객의 모든 데이터를 관리하는 Model
/// View나 Controller에 대한 정보는 알지 못함
/// </summary>
public class Customer
{
    // Events
    public event Action<float> OnPatienceChanged; // 인내심 줄어들고 있음
    public event Action<bool> OnPatienceStateChanged; // 인내심 줄어들어야 하는지 아닌지
    public event Action<FoodData> OnOrderPlaced; // 주문함
    public event Action OnServedStateChanged; // 서빙됨
    public event Action OnEatingStateChanged; // 다 먹음
    public event Action<bool> OnPaymentStateChanged; // 결제 끝남
    
    // 데이터 컨테이너
    private CustomerData _data;
    private CustomerRuntimeData runtimeData;
    
    // === Properties & helper ===
    public CustomerData CustomerData => _data;
    public CustomerRuntimeData CustomerRuntimeData => runtimeData;
    public float GetPatienceRatio() => runtimeData.CurrentPatience / _data.waitTime;

    public void Initialize(CustomerData data)
    {
        _data = data;
        runtimeData = new CustomerRuntimeData(data.waitTime, data.eatTime);
    }

    // Controller가 호출하는 모델 상태변경 메서드. 본인의 상태값만 변경함.
    public void UpdateState(CustomerStateName newState)
    {
        runtimeData.CurrentState = newState;
    }
    
    // 인내심시간 변경, 알림 보내기
    public void UpdatePatience(float newPatience)
    {
        runtimeData.CurrentPatience = newPatience;
        OnPatienceChanged?.Invoke(GetPatienceRatio());
    }

    // 인내심UI 알림 보내기
    public void SetPatienceTimerActive(bool isActive)
    {
        OnPatienceStateChanged?.Invoke(isActive);
    }
    
    // 주문하기
    public void PlaceOrder()
    {
        runtimeData.CurrentOrder = MenuManager.Instance.GetRandomMenu();
        OnOrderPlaced?.Invoke(runtimeData.CurrentOrder);
    }
    
    // 음식 받기
    public void OnFoodServed()
    {
        OnServedStateChanged?.Invoke();
    }
}