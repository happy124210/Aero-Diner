using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class CustomerSpawner : Singleton<CustomerSpawner>
{
    [Header("스폰 세팅")]
    [SerializeField] private float minSpawnInterval = 2f;
    [SerializeField] private float maxSpawnInterval = 5f;
    [SerializeField] private int maxCustomers = 10;
    [SerializeField] private Transform[] spawnPoints;
    
    [Header("레스토랑 설정 - 임시 (나중에 RestaurantManager에서 관리)")]
    [SerializeField] private Transform entrancePoint;
    [SerializeField] private Transform exitPoint;
    
    // 좌석
    [SerializeField] private Transform[] seatPoints;
    [SerializeField] private bool[] seatOccupied;
    
    [Header("줄 서기")]
    [SerializeField] private Transform queueStartPosition;
    [SerializeField] private float queueSpacing = 1f;
    [SerializeField] private int maxQueueLength = 6;
    
    // 줄서기 큐 (앞에서부터 순서대로)
    private Queue<CustomerController> waitingQueue = new Queue<CustomerController>();
    private Dictionary<CustomerController, Vector3> customerQueuePositions = new Dictionary<CustomerController, Vector3>();
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
    
    protected override void Awake()
    {
        base.Awake();
        
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
        if (seatPoints != null && seatPoints.Length > 0)
        {
            seatOccupied = new bool[seatPoints.Length];
            for (int i = 0; i < seatOccupied.Length; i++)
            {
                seatOccupied[i] = false;
            }
        }
        
        waitingQueue.Clear();
        customerQueuePositions.Clear();
        
        if (showDebugInfo)
            Debug.Log($"[CustomerSpawner]: 배열 초기화 완료 - 좌석: {seatPoints?.Length ?? 0}, 최대 큐 길이: {maxQueueLength}");
    }
    
    #endregion

    #region 레스토랑 레이아웃
    
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
        if (isAssigningSeat)
        {
            return false;
        }
        
        isAssigningSeat = true;
        
        try
        {
            if (seatPoints == null || seatPoints.Length == 0 || seatOccupied == null)
            {
                Debug.LogWarning("[CustomerSpawner]: 사용가능 좌석 없음 !!!");
                return false;
            }
            
            // 비어있는 좌석 찾기
            for (int i = 0; i < seatPoints.Length; i++)
            {
                if (!seatOccupied[i] && seatPoints[i])
                {
                    // 좌석 할당
                    seatOccupied[i] = true;
                    customer.SetAssignedSeatPosition(seatPoints[i].position);
                    
                    if (showDebugInfo) Debug.Log($"[CustomerSpawner]: 좌석 {i}번 할당");
                    return true;
                }
            }
            
            if (showDebugInfo) Debug.Log("[CustomerSpawner]: 사용가능 좌석 없음");
            return false;
        }
        finally
        {
            isAssigningSeat = false;
        }
    }
    
    /// <summary>
    /// 줄에 손님 추가
    /// </summary>
    public bool AddCustomerToQueue(CustomerController customer)
    {
        if (waitingQueue.Count >= maxQueueLength)
        {
            Debug.LogWarning("[CustomerSpawner]: 줄이 꽉 찼습니다!");
            return false;
        }
        
        // 큐에 추가
        waitingQueue.Enqueue(customer);
        
        // 줄 위치 계산 및 할당
        Vector3 queuePosition = CalculateQueuePosition(waitingQueue.Count - 1);
        customerQueuePositions[customer] = queuePosition;
        
        customer.UpdateQueuePosition(queuePosition);
        
        if (showDebugInfo) Debug.Log($"[CustomerSpawner]: 줄 {waitingQueue.Count}번째에 합류 (위치: {queuePosition})");
        
        return true;
    }
    
    /// <summary>
    /// 줄에서 손님 제거 및 줄 정렬
    /// </summary>
    public void RemoveCustomerFromQueue(CustomerController customer)
    {
        if (!customer || !customerQueuePositions.ContainsKey(customer))
        {
            if (showDebugInfo) Debug.LogWarning("[CustomerSpawner]: 해당 손님은 줄에 없습니다");
            return;
        }
        
        // 손님을 큐에서 제거
        var tempQueue = new Queue<CustomerController>();
        bool found = false;
        
        while (waitingQueue.Count > 0)
        {
            var queuedCustomer = waitingQueue.Dequeue();
            if (queuedCustomer == customer)
            {
                found = true;
                if (showDebugInfo) Debug.Log("[CustomerSpawner]: 줄에서 손님 빠짐");
            }
            else if (queuedCustomer)
            {
                tempQueue.Enqueue(queuedCustomer);
            }
        }
        
        waitingQueue = tempQueue;
        customerQueuePositions.Remove(customer);
        
        if (found)
        {
            // 줄 재정렬
            ReorganizeQueue();
        }
    }
    
    /// <summary>
    /// 줄 재정렬
    /// </summary>
    private void ReorganizeQueue()
    {
        if (showDebugInfo) Debug.Log("[CustomerSpawner]: 줄 재정렬 시작");
        
        // 기존 위치 정보 초기화
        customerQueuePositions.Clear();
        
        // 큐의 모든 손님들을 새로운 위치로 이동
        var queueArray = waitingQueue.ToArray();
        
        for (int i = 0; i < queueArray.Length; i++)
        {
            var customer = queueArray[i];
            if (!customer) continue;
            
            Vector3 newPosition = CalculateQueuePosition(i);
            customerQueuePositions[customer] = newPosition;
            
            // 손님에게 새로운 위치로 이동하라고 지시
            customer.UpdateQueuePosition(newPosition);
            
            if (showDebugInfo) Debug.Log($"[CustomerSpawner]: 손님을 줄 {i + 1}번째 위치로 이동: {newPosition}");
        }
    }
    
    /// <summary>
    /// 줄 위치 계산 - 동적 Queue 시스템
    /// queueStartPosition에서 시작해서 뒤로 queueSpacing만큼 간격으로 배치
    /// </summary>
    private Vector3 CalculateQueuePosition(int queueIndex)
    {
        if (!queueStartPosition)
        {
            Debug.LogError("[CustomerSpawner]: 줄 시작 위치가 설정되지 않음!");
            return Vector3.zero;
        }
        
        // 시작점에서 뒤로 일정 간격으로 배치
        Vector3 basePosition = queueStartPosition.position;
        return basePosition + Vector3.back * (queueIndex * queueSpacing);
    }
    
    /// <summary>
    /// 줄의 첫 번째 손님이 좌석을 얻을 수 있는지 체크
    /// </summary>
    public CustomerController GetNextCustomerInQueue()
    {
        if (waitingQueue.Count > 0)
        {
            return waitingQueue.Peek(); // 첫 번째 손님 반환
        }
        return null;
    }
    
    /// <summary>
    /// 스폰 가능 여부
    /// </summary>
    public bool CanSpawnNewCustomer()
    {
        // 좌석이 있으면 바로 스폰 가능
        if (GetAvailableSeatCount() > 0)
        {
            return true;
        }
        
        // 좌석이 없어도 줄에 자리가 있으면 스폰 가능
        if (waitingQueue.Count < maxQueueLength)
        {
            return true;
        }
        
        // 둘 다 없으면 스폰 불가
        return false;
    }
    
    /// <summary>
    /// 좌석 해제
    /// </summary>
    public void ReleaseSeat(Vector3 seatPosition)
    {
        if (seatPoints == null || seatOccupied == null) return;
        
        for (int i = 0; i < seatPoints.Length; i++)
        {
            if (seatPoints[i] && Vector3.Distance(seatPoints[i].position, seatPosition) < 0.1f)
            {
                seatOccupied[i] = false;
                if (showDebugInfo) Debug.Log($"[CustomerSpawner]: 좌석 {i}번 해제됨");
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

    /// <summary>
    /// 모든 대기줄 정리
    /// </summary>
    public void ClearAllWaitingLines()
    {
        // Queue 시스템 정리
        waitingQueue.Clear();
        customerQueuePositions.Clear();
        
        if (showDebugInfo) Debug.Log("[CustomerSpawner]: 모든 대기줄 정리됨");
    }
    
    // public getters - Queue 시스템 전용
    public int TotalSeatCount => seatPoints?.Length ?? 0;
    public int CurrentQueueLength => waitingQueue.Count;
    public int MaxQueueLength => maxQueueLength;
    public bool IsQueueFull => waitingQueue.Count >= maxQueueLength;
    
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
            if (PoolManager.Instance.ActiveCustomerCount < maxCustomers && CanSpawnNewCustomer())
            {
                SpawnRandomCustomer();
            }
            
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    public void SpawnRandomCustomer()
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
        if (availableCustomers == null) return null;

        return availableCustomers.FirstOrDefault(customerData => customerData && customerData.rarity == rarity);
    }
    
    #endregion
    
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
        
        ClearAllWaitingLines();
        
        if (showDebugInfo) Debug.Log("[CustomerSpawner]: 모든 손님 정리됨");
    }
    
    #endregion
}