using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : Singleton<PoolManager>
{
    [Header("Pool 설정")]
    [SerializeField, ReadOnly] private Transform poolContainer;
    [SerializeField] private int poolCapacity = 20;
    [SerializeField] private int maxPoolSize = 40;
    
    [Header("리소스 데이터 경로")]
    private const string CUSTOMER_DATA_PATH = "Datas/Customer";
    
    [Header("공통 프리팹")]
    [SerializeField] private GameObject customerPrefab;
    
    [Header("사용가능 손님 타입")]
    [SerializeField, ReadOnly] private CustomerData[] customerTypes;
    
    [Header("Runtime 정보")]
    private Dictionary<CustomerData, ObjectPool<CustomerController>> customerPools = new();
    private Dictionary<CustomerData, Transform> customerPoolParents = new();
    private List<CustomerController> activeCustomers = new();
    
    
    protected override void Awake()
    {
        base.Awake();

        InitializePools();
    }

    /// <summary>
    /// 풀 초기화
    /// </summary>
    private void InitializePools()
    {
        LoadResourceData();
        
        if (poolContainer == null)
        {
            GameObject container = new GameObject("CustomerContainer");
            container.transform.SetParent(poolContainer);
            poolContainer = container.transform;
        }

        // 손님 타입별 pool 생성
        if (customerTypes != null)
        {
            foreach (var customerData in customerTypes)
            {
                if (customerPrefab != null)
                {
                    CreateCustomerPool(customerData);
                }
            }
        }
    }

    /// <summary>
    /// Resources 폴더에서 데이터 로드
    /// </summary>
    private void LoadResourceData()
    {
        customerTypes = Resources.LoadAll<CustomerData>(CUSTOMER_DATA_PATH);

        if (customerTypes.Length == 0)
        {
            Debug.LogError($"[ObjectPoolManager]: 데이터가 Resources/{CUSTOMER_DATA_PATH}에 있는 거 맞나요???");
        }
    }

    #region 손님 풀 데이터

    /// <summary>
    /// 손님 풀 생성
    /// </summary>
    private void CreateCustomerPool(CustomerData customerData)
    {
        // 부모 오브젝트 생성
        GameObject poolParent = new GameObject($"CustomerPool_{customerData.customerName}");
        poolParent.transform.SetParent(poolContainer);
        customerPoolParents[customerData] = poolParent.transform;
        
        // Unity ObjectPool 생성
        customerPools[customerData] = new ObjectPool<CustomerController>(
            createFunc: () => CreateNewCustomer(customerData),
            actionOnGet: (customer) => OnGetCustomer(customer, customerData),
            actionOnRelease: (customer) => OnReleaseCustomer(customer, customerData),
            actionOnDestroy: (customer) => DestroyCustomer(customer),
            collectionCheck: true,
            defaultCapacity: poolCapacity,
            maxSize: maxPoolSize
        );
        
        Debug.Log($"[ObjectPoolManager]: {customerData.customerName} 손님 풀 생성 완료");
    }


    private CustomerController CreateNewCustomer(CustomerData customerData)
    {
        GameObject customerObj = Instantiate(customerPrefab, customerPoolParents[customerData]);
        CustomerController customer = customerObj.GetComponent<CustomerController>();
        
        if (!customer)
        {
            Debug.LogError("[ObjectPoolManager]: CustomerController 컴포넌트 없음 !!!");
            Destroy(customerObj);
            return null;
        }
        
        return customer;
    }

    /// <summary>
    /// (풀 내부용) 손님 활성화
    /// </summary>
    private void OnGetCustomer(CustomerController customer, CustomerData customerData)
    {
        if (!customer) return;
        
        customer.gameObject.SetActive(true);
        customer.InitializeFromPool(customerData);
        activeCustomers.Add(customer);
    }

    /// <summary>
    /// (풀 내부용) 손님 비활성화
    /// </summary>
    private void OnReleaseCustomer(CustomerController customer, CustomerData customerData)
    {
        if (!customer) return;

        if (activeCustomers.Contains(customer))
        {
            activeCustomers.Remove(customer);
        }
        
        customer.gameObject.SetActive(false);
        customer.transform.SetParent(customerPoolParents[customerData]);
    }
    
    /// <summary>
    /// 손님 파괴
    /// </summary>
    private void DestroyCustomer(CustomerController customer)
    {
        if (!customer) return;
        
        Destroy(customer.gameObject);
    }

    /// <summary>
    /// 풀에서 손님 스폰
    /// </summary>
    /// <param name="customerData"> 스폰할 손님 데이터 </param>
    /// <param name="position"> 스폰 위치 </param>
    /// <param name="rotation"> 회전값 </param>
    /// <returns></returns>
    public void SpawnCustomer(CustomerData customerData, Vector3 position, Quaternion rotation = default)
    {
        if (!customerData)
        {
            Debug.LogError("[ObjectPoolManager]: 손님 데이터 없음 !!!");
        }

        if (!customerPools.ContainsKey(customerData))
        {
            CreateCustomerPool(customerData);
        }

        CustomerController customer = customerPools[customerData].Get();

        if (customer)
        {
            customer.transform.position = position;
            customer.transform.rotation = rotation == default ? Quaternion.identity : rotation;
        }
        
        CustomerManager.Instance.AddCustomer(customer);
    }

    /// <summary>
    /// 손님 풀로 반환
    /// </summary>
    /// <param name="customer"></param>
    public void DespawnCustomer(CustomerController customer)
    {
        if (!customer) return;

        CustomerData customerData = customer.CustomerData;
        if (customerData && customerPools.ContainsKey(customerData))
        {
            customerPools[customerData].Release(customer);
            CustomerManager.Instance.RemoveCustomer(customer);
        }
        else
        {
            if (activeCustomers.Contains(customer))
            {
                activeCustomers.Remove(customer);
            }
            Debug.LogError($"[ObjectPoolManager]: {customer.name} 손님 데이터 없음 !!!");
            Destroy(customer.gameObject);
        }
    }
    
    /// <summary>
    /// 모든 활성 손님 반환
    /// </summary>
    public void ReturnAllActiveCustomers()
    {
        List<CustomerController> customersToReturn = new List<CustomerController>(activeCustomers);
        
        foreach (var customer in customersToReturn)
        {
            DespawnCustomer(customer);
        }
        
        Debug.Log("[ObjectPoolManager] : 모든 활성 손님을 풀로 반환!");
    }

    #endregion

    #region public getters

    public int ActiveCustomerCount => activeCustomers.Count;
    public CustomerData[] AvailableCustomers => customerTypes;

    #endregion
}
