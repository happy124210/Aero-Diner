using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CustomerSpawner : MonoBehaviour
{
    [Header("스폰 세팅")]
    [SerializeField] private float minSpawnInterval = 2f;
    [SerializeField] private float maxSpawnInterval = 5f;
    [SerializeField] private int maxCustomers = 10;
    [SerializeField] private Transform[] spawnPoints;
    
    // 줄서기 큐 (앞에서부터 순서대로)
    private Queue<CustomerController> waitingQueue = new Queue<CustomerController>();
    private readonly Dictionary<CustomerController, Vector3> customerQueuePositions = new Dictionary<CustomerController, Vector3>();
    private bool isAssigningSeat;
    
    [Header("손님 타입 리스트 (자동 로드됨)")]
    [SerializeField] private List<string> customerDataIds = new List<string>();
    
    [Header("스폰 확률 - 임시")]
    [SerializeField] private float normalCustomerChance = 0.6f;
    [SerializeField] private float rareCustomerChance = 0.2f;
    
    [Header("Debug")]
    [SerializeField] private bool autoSpawn = true;
    [SerializeField] private bool showDebugInfo = true;
    
    private Coroutine spawnCoroutine;

    #region Unity events
    
    private void Awake()
    {
        CustomerData[] customerDatas = Resources.LoadAll<CustomerData>("Datas/Customer");

        foreach (CustomerData customerData in customerDatas)
        {
            customerDataIds.Add(customerData.id);
        }
        
        InitializeArrays();
    }

    private void Start()
    {
        if (autoSpawn)
        {
            StartSpawning();
        }
    }
    
    private void OnDestroy()
    {
        StopSpawning();
    }
    
    #endregion

    #region 초기화
    
    /// <summary>
    /// 배열들 초기화
    /// </summary>
    private void InitializeArrays()
    {
        waitingQueue.Clear();
        customerQueuePositions.Clear();
    }
    
    #endregion
    
    #region 스폰 시스템
    
    public void StartSpawning()
    {
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnCustomerCoroutine());
            if (showDebugInfo) Debug.Log("[CustomerSpawner]: 자동 스폰 시작");
        }
    }
    
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
            if (showDebugInfo) Debug.Log("[CustomerSpawner]: 자동 스폰 중단");
        }
    }

    private IEnumerator SpawnCustomerCoroutine()
    {
        while (true)
        {
            if (PoolManager.Instance.ActiveCustomerCount < maxCustomers && TableManager.Instance.CanAcceptNewCustomer())
            {
                SpawnRandomCustomer();
            }
            
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void SpawnRandomCustomer()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[CustomerSpawner]: 스폰 지점 설정해주세요 !!!");
            return;
        }

        // 스폰 포인트 중 랜덤으로 선택
        Vector3 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        
        CustomerData customerData = SelectRandomCustomer();
        if (!customerData)
        {
            // 확률 선택 실패 시 일반 랜덤
            var availableCustomers = PoolManager.Instance.AvailableCustomers;
            if (availableCustomers != null && availableCustomers.Length > 0)
            {
                customerData = availableCustomers[Random.Range(0, availableCustomers.Length)];
            }
        }

        if (customerData)
        {
            CustomerController selectedCustomer = PoolManager.Instance.SpawnCustomer(customerData, spawnPosition);
            if (selectedCustomer && showDebugInfo) 
                Debug.Log($"[CustomerSpawner]: {customerData.customerName} 스폰 완료!"); 
        }
    }

    private CustomerData SelectRandomCustomer()
    {
        float random = Random.Range(0f, 1f);

        // 확률별 선택 로직
        if (random < normalCustomerChance)
        {
            return FindCustomerByRarity(CustomerRarity.Normal);
        }
        else
        {
            return FindCustomerByRarity(CustomerRarity.Rare);
        }
    }

    private CustomerData FindCustomerByRarity(CustomerRarity rarity)
    {
        var availableCustomers = PoolManager.Instance.AvailableCustomers;

        return availableCustomers?.FirstOrDefault(customerData => customerData && customerData.rarity == rarity);
    }
    
    #endregion
    
    #region Manual Control (디버그용)
    
    [ContextMenu("Spawn Random Customer")]
    public void SpawnSingleCustomer()
    {
        SpawnRandomCustomer();
    }
    
    #endregion
}