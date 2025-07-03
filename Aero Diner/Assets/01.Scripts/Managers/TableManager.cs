using System.Collections.Generic;
using UnityEngine;

public class TableManager : Singleton<TableManager>
{
    [Header("테이블 설정")]
    [SerializeField] private Table[] tables;
    [SerializeField] private bool[] seatOccupied;
    
    [Header("줄서기 설정")]
    [SerializeField] private Transform queueStartPosition;
    [SerializeField] private float queueSpacing = 1f;
    [SerializeField] private int maxQueueLength = 6;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    // 줄서기 큐
    private Queue<CustomerController> waitingQueue = new();
    private readonly Dictionary<CustomerController, Vector3> customerQueuePositions = new();

    #region Unity Events

    protected override void Awake()
    {
        base.Awake();
        InitializeTables();
    }

    #endregion

    #region 초기화

    private void InitializeTables()
    {
        if (tables == null || tables.Length == 0)
        {
            Debug.LogError("[TableManager] Table 배열 설정 안 됨 !!!");
            return;
        }

        seatOccupied = new bool[tables.Length];
        
        for (int i = 0; i < tables.Length; i++)
        {
            seatOccupied[i] = false;
            
            if (tables[i] != null)
            {
                tables[i].SetSeatIndex(i);
            }
        }
        
        // 줄서기 초기화
        waitingQueue.Clear();
        customerQueuePositions.Clear();
        
        if (showDebugInfo)
            Debug.Log($"[TableManager]: 테이블 초기화 완료 - 총 {tables.Length}개");
    }

    #endregion

    #region 좌석 관리

    /// <summary>
    /// 좌석 할당 시도
    /// </summary>
    /// <returns> 성공 시 true, 실패 시 false </returns>
    public bool TryAssignSeat(CustomerController customer)
    {
        if (AssignSeatToCustomer(customer))
        {
            return true;
        }
        
        // 좌석 없으면 줄에 추가
        return AddCustomerToQueue(customer);
    }

    /// <summary>
    /// 직접 좌석 할당
    /// </summary>
    private bool AssignSeatToCustomer(CustomerController customer)
    {
        if (tables == null || seatOccupied == null)
        {
            Debug.LogError("[TableManager]: 테이블 또는 좌석 배열이 null입니다!");
            return false;
        }

        for (int i = 0; i < tables.Length; i++)
        {
            if (!seatOccupied[i] && tables[i])
            {
                seatOccupied[i] = true;
                
                customer.SetAssignedTable(tables[i]);
                tables[i].AssignCustomer(customer);
                
                if (showDebugInfo)
                    Debug.Log($"[TableManager]: 테이블 {i}번 할당 완료 - {customer.name}");
                
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 좌석 해제, 다음 손님 할당
    /// </summary>
    public void ReleaseSeat(CustomerController customer)
    {
        if (!customer) return;
        
        Table customerTable = customer.GetAssignedTable();
        if (!customerTable) return;
        
        for (int i = 0; i < tables.Length; i++)
        {
            if (tables[i] == customerTable)
            {
                seatOccupied[i] = false;
                tables[i].ReleaseCustomer();
                customer.SetAssignedTable(null);
                
                if (showDebugInfo)
                    Debug.Log($"[TableManager]: 테이블 {i}번 해제 완료 - {customer.name}");
                
                AssignNextCustomerFromQueue();
                break;
            }
        }
    }

    #endregion

    #region 줄서기 관리

    /// <summary>
    /// 줄에 손님 추가
    /// </summary>
    private bool AddCustomerToQueue(CustomerController customer)
    {
        if (waitingQueue.Count >= maxQueueLength)
        {
            Debug.LogWarning("[TableManager]: 줄이 꽉 찼습니다!");
            return false;
        }
        
        waitingQueue.Enqueue(customer);
        Vector3 queuePosition = CalculateQueuePosition(waitingQueue.Count - 1);
        customerQueuePositions[customer] = queuePosition;
        
        // 손님을 줄서기 상태로 변경
        customer.StartWaitingInLine(queuePosition);
        
        if (showDebugInfo) 
            Debug.Log($"[TableManager]: 줄 {waitingQueue.Count}번째에 합류 - {customer.name}");
        
        return true;
    }

    /// <summary>
    /// 줄에서 다음 손님을 자동으로 좌석에 할당
    /// </summary>
    private void AssignNextCustomerFromQueue()
    {
        if (waitingQueue.Count == 0) return;
        
        CustomerController nextCustomer = waitingQueue.Dequeue();
        customerQueuePositions.Remove(nextCustomer);
        
        // 바로 좌석 할당
        if (AssignSeatToCustomer(nextCustomer))
        {
            // 손님을 좌석으로 이동 상태로 변경
            nextCustomer.MoveToAssignedSeat();
            
            // 줄 재정렬
            ReorganizeQueue();
            
            if (showDebugInfo)
                Debug.Log($"[TableManager]: 줄에서 좌석으로 자동 할당 - {nextCustomer.name}");
        }
        else
        {
            Debug.LogError("[TableManager]: 좌석 할당 실패! 줄에서 제거된 손님을 다시 추가");
            // 예외처리
        }
    }

    /// <summary>
    /// 줄에서 손님 제거
    /// </summary>
    public void RemoveCustomerFromQueue(CustomerController customer)
    {
        if (!customerQueuePositions.ContainsKey(customer)) return;
        
        var tempQueue = new Queue<CustomerController>();
        bool found = false;
        
        while (waitingQueue.Count > 0)
        {
            var queuedCustomer = waitingQueue.Dequeue();
            if (queuedCustomer == customer)
            {
                found = true;
                if (showDebugInfo) 
                    Debug.Log($"[TableManager]: 줄에서 손님 제거 - {customer.name}");
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
            ReorganizeQueue();
        }
    }

    /// <summary>
    /// 줄 재정렬
    /// </summary>
    private void ReorganizeQueue()
    {
        customerQueuePositions.Clear();
        var queueArray = waitingQueue.ToArray();
        
        for (int i = 0; i < queueArray.Length; i++)
        {
            var customer = queueArray[i];
            if (!customer) continue;
            
            Vector3 newPosition = CalculateQueuePosition(i);
            customerQueuePositions[customer] = newPosition;
            customer.UpdateQueuePosition(newPosition);
        }
    }

    /// <summary>
    /// 줄 위치 계산
    /// </summary>
    private Vector3 CalculateQueuePosition(int queueIndex)
    {
        if (!queueStartPosition)
        {
            Debug.LogError("[TableManager]: 줄 시작 위치가 설정되지 않음!");
            return Vector3.zero;
        }
        
        Vector3 basePosition = queueStartPosition.position;
        return basePosition + Vector3.left * (queueIndex * queueSpacing);
    }

    #endregion

    #region 상태 체크
    
    public bool CanAcceptNewCustomer() 
    {
        // 좌석이 있거나 줄에 자리가 있으면
        return HasAvailableSeat() || waitingQueue.Count < maxQueueLength;
    }

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

    #endregion

    #region 정리

    /// <summary>
    /// 모든 좌석과 줄 해제
    /// </summary>
    public void ReleaseAllSeatsAndQueue()
    {
        // 좌석 해제
        if (seatOccupied != null)
        {
            for (int i = 0; i < seatOccupied.Length; i++)
            {
                seatOccupied[i] = false;
            }
        }
        
        if (tables != null)
        {
            foreach (var table in tables)
            {
                if (table != null)
                {
                    table.ReleaseCustomer();
                }
            }
        }
        
        // 줄 정리
        waitingQueue.Clear();
        customerQueuePositions.Clear();
        
        if (showDebugInfo)
            Debug.Log("[TableManager]: 모든 좌석과 줄 해제됨");
    }

    #endregion

    #region Public Getters

    public int TotalSeatCount => tables?.Length ?? 0;
    public int CurrentQueueLength => waitingQueue.Count;
    public int MaxQueueLength => maxQueueLength;

    public bool IsQueueFull => waitingQueue.Count >= maxQueueLength;
	public bool HasAvailableSeat() => GetAvailableSeatCount() > 0;

    #endregion
    
}