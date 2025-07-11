using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : Singleton<CustomerManager>
{
    [Header("Debug Info")]
    [SerializeField] bool showDebugInfo;

    private readonly List<CustomerController> activeCustomers = new();
    public IReadOnlyList<CustomerController> ActiveCustomers => activeCustomers;

    /// <summary>
    /// 활성 손님 리스트에 추가
    /// </summary>
    public void AddCustomer(CustomerController customer)
    {
        if (!activeCustomers.Contains(customer))
        {
            activeCustomers.Add(customer);
            
            if (showDebugInfo) Debug.Log($"[Customer Manager] : {customer.name} 추가됨");
        }
    }

    /// <summary>
    /// 활성 손님 리스트에서 제거
    /// </summary>
    public void RemoveCustomer(CustomerController customer)
    {
        if (activeCustomers.Contains(customer))
        {
            activeCustomers.Remove(customer);
            
            if (showDebugInfo) Debug.Log($"[Customer Manager] : {customer.name} 제거됨");
        }
    }
    
    /// <summary>
    /// 모든 활성 손님 떠나게 하기
    /// </summary>
    public void ForceAllCustomersToLeave()
    {
        List<CustomerController> customersToLeave = new List<CustomerController>(activeCustomers);
        foreach (var customer in customersToLeave)
        {
            if (customer.CurrentStateName != CustomerStateName.Leaving)
            {
                customer.ForceLeave();
            }
        }
        
        if (showDebugInfo) Debug.Log("[CustomerManager]: 모든 손님 쫓아냄 !!");
    }
    
}
