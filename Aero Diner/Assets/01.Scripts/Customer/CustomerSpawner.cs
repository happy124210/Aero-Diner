using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CustomerSpawner : Singleton<CustomerSpawner>
{
    [Header("스폰 세팅")]
    [SerializeField] private float minSpawnInterval;
    [SerializeField] private float maxSpawnInterval;
    [SerializeField] private int maxCustomers;
    [SerializeField] private Transform[] spawnPoints;
    
    [Header("레스토랑 설정 - 임시 (나중에 RestaurantManager에서 관리)")]
    [SerializeField] private Transform entrancePoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private Transform[] seatPoints;
    [SerializeField] private bool[] seatOccupied;
    
    [Header("손님 타입 리스트 (자동 로드됨)")]
    [SerializeField] private List<string> customerDataIds = new List<string>();
    
    [Header("스폰 확률 - 임시")]
    [SerializeField] private float normalCustomerChance = 1f;
    [SerializeField] private float rareCustomerChance = 0f;
    
    [Header("Debug")]
    [SerializeField] private bool autoSpawn = true;
    [SerializeField] private bool showSpawnInfo = true;
    
    private Coroutine spawnCoroutine;

    #region Unity events
    
    private void Awake()
    {
        CustomerData[] customerDatas = Resources.LoadAll<CustomerData>("Datas/Customer");

        foreach (CustomerData customerData in customerDatas)
        {
            customerDataIds.Add(customerData.id);
        }
    }

    private void Start()
    {
        if (autoSpawn)
        {
            StartSpawning();
        }
    }
    
    #endregion

    #region 레스토랑 레이아웃 - 임시
    
    /// <summary>
    /// 입구 위치 반환
    /// </summary>
    public Vector3 GetEntrancePosition()
    {
        return entrancePoint ? entrancePoint.position : Vector3.zero;
    }
    
    /// <summary>
    /// 출구 위치 반환
    /// </summary>
    public Vector3 GetExitPosition()
    {
        return exitPoint ? exitPoint.position : Vector3.zero;
    }
    
    /// <summary>
    /// 손님에게 좌석 할당
    /// </summary>
    public bool AssignSeatToCustomer(CustomerController customer)
    {
        if (seatPoints == null || seatPoints.Length == 0)
        {
            Debug.LogWarning("No seat points available!");
            return false;
        }
        
        // 비어있는 좌석 찾기
        for (int i = 0; i < seatPoints.Length; i++)
        {
            if (!seatOccupied[i])
            {
                // 좌석 할당
                seatOccupied[i] = true;
                customer.SetAssignedSeatPosition(seatPoints[i].position);
                
                Debug.Log($"Seat {i} assigned to customer");
                return true;
            }
        }
        
        Debug.Log("No available seats!");
        return false;
    }
    
    /// <summary>
    /// 좌석 해제 (손님이 떠날 때)
    /// </summary>
    public void ReleaseSeat(Vector3 seatPosition)
    {
        if (seatPoints == null) return;
        
        for (int i = 0; i < seatPoints.Length; i++)
        {
            if (Vector3.Distance(seatPoints[i].position, seatPosition) < 0.1f)
            {
                seatOccupied[i] = false;
                Debug.Log($"Seat {i} released");
                break;
            }
        }
    }
    
    /// <summary>
    /// 사용 가능한 좌석 수 반환
    /// </summary>
    public int GetAvailableSeatCount()
    {
        if (seatOccupied == null) return 0;
        
        int availableCount = 0;
        foreach (bool occupied in seatOccupied)
        {
            if (!occupied) availableCount++;
        }
        return availableCount;
    }
    
    // public getters
    public int TotalSeatCount => seatPoints?.Length ?? 0;
    
    #endregion
    
    
    public void StartSpawning()
    {
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnCustomerCoroutine());
        }
    }
    
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnCustomerCoroutine()
    {
        while (true)
        {
            if (PoolManager.Instance.ActiveCustomerCount < maxCustomers)
            {
                SpawnRandomCustomer();
            }
        }
    }

    public void SpawnRandomCustomer()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[CustomerSpawner]: 스폰 지점 설정해주세요 !!!");
        }

        // 스폰 포인트 중 랜덤으로 선택
        Vector3 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        
        
        CustomerData customerData = SelectRandomCustomer();
        if (!customerData)
        {
            // 데이터 없을 경우 처리
        }

        CustomerController selectedCustomer = PoolManager.Instance.SpawnCustomer(customerData, spawnPosition);
        if (selectedCustomer && showSpawnInfo) Debug.Log("[CustomerSpawner]: 스폰 완료!"); 
    }

    private CustomerData SelectRandomCustomer()
    {
        float random = Random.Range(0f, 1f);

        // 확률별 선택 로직
        if (random < normalCustomerChance)
        {
            return FindCustomerByRarity(CustomerRarity.Normal);
        }

        if (random < normalCustomerChance + rareCustomerChance)
        {
            return FindCustomerByRarity(CustomerRarity.Rare);
        }

        return FindCustomerByRarity(CustomerRarity.Special);
    }

    private CustomerData FindCustomerByRarity(CustomerRarity rarity)
    {
        var availableCustomers = PoolManager.Instance.AvailableCustomers;

        return availableCustomers.FirstOrDefault(customerData => customerData && customerData.rarity == rarity);
    }
    
    #region Manual Control (디버그용)
    
    [ContextMenu("Spawn Random Customer")]
    public void SpawnSingleCustomer()
    {
        SpawnRandomCustomer();
    }
    
    [ContextMenu("Clear All Customers")]
    public void ClearAllCustomers()
    {
        PoolManager.Instance.ReturnAllActiveCustomers();
        
        // 모든 좌석 해제
        if (seatOccupied != null)
        {
            for (int i = 0; i < seatOccupied.Length; i++)
            {
                seatOccupied[i] = false;
            }
        }
    }
    
    #endregion
}
