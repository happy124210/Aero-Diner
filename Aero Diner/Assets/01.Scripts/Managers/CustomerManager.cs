using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomerManager : Singleton<CustomerManager>
{
    [Header("손님 프리팹")]
    [SerializeField] private GameObject customerPrefab;
    
    [Header("Debug Info")]
    [SerializeField] private bool showDebugInfo;
    
    private CustomerData[] availableCustomerTypes;
    private readonly List<CustomerController> activeCustomers = new();

    public int ActiveCustomerCount => activeCustomers.Count;
    public IReadOnlyList<CustomerData> AvailableCustomerTypes => availableCustomerTypes;

    protected override void Awake()
    {
        base.Awake();
        LoadResourceData();
    }

    private void LoadResourceData()
    {
        customerPrefab = Resources.Load<GameObject>(StringPath.CUSTOMER_PREFAB_PATH);
        availableCustomerTypes = Resources.LoadAll<CustomerData>(StringPath.CUSTOMER_DATA_PATH);
    }
    
    public CustomerController SpawnCustomer(CustomerData data, Vector3 position, Quaternion rotation)
    {
        if (!customerPrefab) return null;

        GameObject customerObj = PoolManager.Instance.Get(customerPrefab, position, rotation);
        if (!customerObj.TryGetComponent<CustomerController>(out var controller))
        {
            PoolManager.Instance.Release(customerPrefab, customerObj);
            return null;
        }

        controller.Setup(data);
        activeCustomers.Add(controller);
        return controller;
    }

    public void DespawnCustomer(CustomerController customer)
    {
        if (!customer) return;
        activeCustomers.Remove(customer);
        PoolManager.Instance.Release(customerPrefab, customer.gameObject);
    }
    
    /// <summary>
    /// 현재 활성화된 모든 손님의 인내심 0으로 만들기
    /// </summary>
    [ContextMenu("Debug/모든 손님 인내심 제거")]
    public void EmptyAllPatience()
    {
        if (activeCustomers.Count == 0)
        {
            Debug.Log("[CustomerManager] 활성화된 손님이 없습니다.");
            return;
        }

        Debug.LogWarning($"[CustomerManager] {activeCustomers.Count}명의 모든 손님의 인내심을 제거합니다.");
        
        foreach (var customer in activeCustomers.ToList())
        {
            customer.EmptyPatience();
        }
    }

    /// <summary>
    /// 현재 활성화된 모든 손님을 풀로 반환
    /// </summary>
    [ContextMenu("Debug/모든 손님 풀로 반환")]
    public void DespawnAllCustomers()
    {
        if (activeCustomers.Count == 0)
        {
            if (showDebugInfo) Debug.Log("[CustomerManager] 활성화된 손님 없음");
            return;
        }

        Debug.LogWarning($"[CustomerManager] {activeCustomers.Count}명의 모든 손님을 풀로 반환합니다.");
        
        foreach (var customer in activeCustomers.ToList())
        {
            DespawnCustomer(customer);
        }
    }
}