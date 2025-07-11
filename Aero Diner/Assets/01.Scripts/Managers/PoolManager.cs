using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : Singleton<PoolManager>
{
    [Header("Pool 설정")]
    [SerializeField] private Transform poolContainer;
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
    private readonly List<CustomerController> activeCustomers = new();
    
    #region properties
    
    public CustomerData[] CustomerTypes => customerTypes;
    
    #endregion
    
    protected override void Awake()
    {
        base.Awake();
        InitializePools();
    }

    private void InitializePools()
    {
        LoadResourceData();
        
        // 컨테이너가 없으면 PoolManager의 자식으로 생성
        if (poolContainer == null)
        {
            GameObject container = new GameObject("PoolContainer");
            poolContainer = container.transform;
            poolContainer.SetParent(transform);
        }

        // 미리 정의된 손님 타입에 대한 풀을 미리 생성
        if (customerTypes != null && customerPrefab != null)
        {
            foreach (var customerData in customerTypes)
            {
                CreateCustomerPool(customerData);
            }
        }
    }
    
    private void LoadResourceData()
    {
        customerTypes = Resources.LoadAll<CustomerData>(CUSTOMER_DATA_PATH);
        if (customerTypes.Length == 0)
        {
            Debug.LogError($"[PoolManager] Resources/{CUSTOMER_DATA_PATH} 경로에 CustomerData가 없습니다.");
        }
    }

    #region 손님 풀링 로직

    private void CreateCustomerPool(CustomerData customerData)
    {
        // 이미 해당 풀이 있으면 생성하지 않음
        if (customerPools.ContainsKey(customerData)) return;

        var poolParent = new GameObject($"Pool_{customerData.customerName}").transform;
        poolParent.SetParent(poolContainer);
        customerPoolParents[customerData] = poolParent;
        
        var newPool = new ObjectPool<CustomerController>(
            createFunc: () => CreateNewCustomer(customerData),
            actionOnGet: (customer) => OnGetCustomer(customer, customerData),
            actionOnRelease: (customer) => OnReleaseCustomer(customer, customerData),
            actionOnDestroy: (customer) => DestroyCustomer(customer),
            collectionCheck: true,
            defaultCapacity: poolCapacity,
            maxSize: maxPoolSize
        );
        
        customerPools[customerData] = newPool;
        Debug.Log($"[PoolManager] {customerData.customerName} 손님 풀 생성 완료");
    }

    private CustomerController CreateNewCustomer(CustomerData customerData)
    {
        // 생성 시 부모 지정
        GameObject customerObj = Instantiate(customerPrefab, customerPoolParents[customerData]);
        if (!customerObj.TryGetComponent<CustomerController>(out var customer))
        {
            Debug.LogError("[PoolManager] CustomerController 없음 !!!", customerObj);
            Destroy(customerObj);
            return null;
        }
        return customer;
    }

    private void OnGetCustomer(CustomerController customer, CustomerData customerData)
    {
        if (!customer) return;
        
        // 활성화 및 데이터 초기화
        customer.gameObject.SetActive(true);
        customer.InitializeFromPool(customerData);
        
        // 활성 리스트에 추가
        activeCustomers.Add(customer);
    }

    private void OnReleaseCustomer(CustomerController customer, CustomerData customerData)
    {
        if (!customer) return;

        // 활성 리스트에서 제거
        activeCustomers.Remove(customer);
        
        // 풀로 돌아갈 때 처리
        customer.OnReturnToPool();
        customer.transform.SetParent(customerPoolParents[customerData]);
    }
    
    private void DestroyCustomer(CustomerController customer)
    {
        if (customer)
        {
            Destroy(customer.gameObject);
        }
    }

    public void SpawnCustomer(CustomerData customerData, Vector3 position, Quaternion rotation = default)
    {
        if (!customerData)
        {
            Debug.LogError("[PoolManager] 손님 데이터 없음 !!!");
            return;
        }

        // 풀이 없으면 즉시 생성
        if (!customerPools.ContainsKey(customerData))
        {
            Debug.LogWarning($"[PoolManager] {customerData.name} 풀 없어서 새로 생성함");
            CreateCustomerPool(customerData);
        }

        CustomerController customer = customerPools[customerData].Get();

        if (customer)
        {
            customer.transform.SetPositionAndRotation(position, rotation == default ? Quaternion.identity : rotation);
        }
    }

    public void DespawnCustomer(CustomerController customer)
    {
        if (!customer) return;

        CustomerData data = customer.CustomerData;
        
        if (data && customerPools.TryGetValue(data, out var pool))
        {
            pool.Release(customer);
        }
        else
        {
            Debug.LogWarning($"[PoolManager] '{customer.name}' 풀에 없음 !!!");
            activeCustomers.Remove(customer);
            Destroy(customer.gameObject);
        }
    }
    
    public void ReturnAllActiveCustomers()
    {
        var customersToReturn = new List<CustomerController>(activeCustomers);
        
        foreach (var customer in customersToReturn)
        {
            customer.ForceLeave();
        }
        
        Debug.Log($"[PoolManager] 모든 활성 손님({customersToReturn.Count}명) 쫓아내기");
    }

    #endregion

    #region Public Getters
    public int ActiveCustomerCount => activeCustomers.Count;
    public IReadOnlyCollection<CustomerData> AvailableCustomers => customerTypes;
    #endregion
}