using System;
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
    [SerializeField] private int maxCustomers;
    [SerializeField] private Transform[] spawnPoints;
    
    [Header("손님 타입 리스트 (자동 로드됨)")]
    [SerializeField] private List<string> customerDataIds = new List<string>();
    
    [Header("스폰 확률 - 임시")]
    [SerializeField] private float normalCustomerChance = 0.6f;
    [SerializeField] private float rareCustomerChance = 0.2f;
    
    [Header("Debug")]
    [SerializeField] private bool autoSpawn = true;
    [SerializeField] private bool showSpawnInfo = true;
    
    private Coroutine spawnCoroutine;

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

    private void StartSpawning()
    {
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnCustomerCoroutine());
        }
    }
    
    private void StopSpawning()
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
}
