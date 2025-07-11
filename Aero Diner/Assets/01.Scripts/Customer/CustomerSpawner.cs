using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CustomerSpawner : MonoBehaviour
{
    [Header("스폰 세팅")]
    [SerializeField] private float minSpawnInterval;
    [SerializeField] private float maxSpawnInterval;
    [SerializeField] private int maxCustomers = 10;
    [SerializeField] private Transform[] spawnPoints;
    
    [Header("스폰 확률")]
    [Range(0f, 1f)]
    [SerializeField] private float normalCustomerChance;
    
    [Header("Debug")]
    [SerializeField] private bool autoSpawn = true;
    [SerializeField] private bool showDebugInfo = true;
    
    private Coroutine spawnCoroutine;
    private CustomerData[] availableCustomers;

    #region Unity events
    
    private void Start()
    {
        availableCustomers = PoolManager.Instance.CustomerTypes;
    }
    
    private void OnDestroy()
    {
        StopSpawning();
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
        while (autoSpawn)
        {
            if (PoolManager.Instance.ActiveCustomerCount < maxCustomers && 
                TableManager.Instance.CanAcceptNewCustomer())
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
            Debug.LogError("[CustomerSpawner]: 스폰 지점을 설정해주세요!");
            return;
        }

        // 스폰 포인트 중 랜덤으로 선택
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        CustomerData customerData = SelectRandomCustomerByRarity();
        if (!customerData)
        {
             if (showDebugInfo) Debug.LogWarning("[CustomerSpawner]: 스폰할 손님 데이터를 찾지 못했습니다. 스폰을 건너뜁니다.");
             return;
        }

        PoolManager.Instance.SpawnCustomer(customerData, spawnPoint.position, spawnPoint.rotation);
        if (showDebugInfo) 
            Debug.Log($"[CustomerSpawner]: {customerData.customerName} (등급: {customerData.rarity}) 스폰 완료!"); 
    }

    private CustomerData SelectRandomCustomerByRarity()
    {
        float randomValue = Random.value;
        
        CustomerRarity selectedRarity = (randomValue < normalCustomerChance) ? CustomerRarity.Normal : CustomerRarity.Rare;
        
        List<CustomerData> candidates = availableCustomers
            .Where(c => c && c.rarity == selectedRarity)
            .ToList();
        
        if (candidates.Count == 0)
        {
            selectedRarity = (selectedRarity == CustomerRarity.Normal) ? CustomerRarity.Rare : CustomerRarity.Normal;
            candidates = availableCustomers
                .Where(c => c && c.rarity == selectedRarity)
                .ToList();
        }
        
        return candidates.Count == 0 ? null : candidates[Random.Range(0, candidates.Count)];
    }

    #endregion
    
    public void SpawnSingleCustomer()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("게임 실행 중에만 손님을 스폰할 수 있습니다.");
            return;
        }
        SpawnRandomCustomer();
    }
}