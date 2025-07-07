using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class ResultPanel : MonoBehaviour
{
    [Header("Sales")] 
    [SerializeField] private GameObject contentPrefab;
    [SerializeField] Transform contentTransform;
    [SerializeField] TextMeshProUGUI totalSalesVolume;
    [SerializeField] TextMeshProUGUI totalRevenue;
    
    [Header("Customer Result")]
    [SerializeField] TextMeshProUGUI allCustomer;
    [SerializeField] TextMeshProUGUI servedCustomer;
    [SerializeField] TextMeshProUGUI goneCustomer;

    private void Reset()
    {
        contentTransform = transform.Find("Content");
        totalSalesVolume = transform.FindChild<TextMeshProUGUI>("Tmp_TotalSalesVolume");
        totalRevenue = transform.FindChild<TextMeshProUGUI>("Tmp_TotalRevenue");
        
        allCustomer = transform.FindChild<TextMeshProUGUI>("Tmp_AllCustomer");
        servedCustomer = transform.FindChild<TextMeshProUGUI>("Tmp_ServedCustomer");
        goneCustomer = transform.FindChild<TextMeshProUGUI>("Tmp_GoneCustomer");
    }
    public void Init()
    {
        SetSalesResult();
        SetCustomerResult();
    }

    private void OnEnable()
    {
        Init();
    }

    private void SetSalesResult()
    {
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        var salesResults = MenuManager.Instance.GetAllMenuSalesData();

        if (salesResults != null)
        {
            foreach (var sales in salesResults)
            {
                var go = Instantiate(contentPrefab, contentTransform);
                var foodUI = go.GetComponent<ResultPanelContent>();
                foodUI.SetData(sales);
            }
        }
        
        totalSalesVolume.text = MenuManager.Instance.GetTotalSalesToday().ToString();
        totalRevenue.text = MenuManager.Instance.GetTotalRevenueToday().ToString();
    }
    
    private void SetCustomerResult()
    {
        int all = RestaurantManager.Instance.CustomersVisited;
        int served = RestaurantManager.Instance.CustomersServed;
        int gone = all - served;
        
        allCustomer.text = all.ToString();
        servedCustomer.text = served.ToString();
        goneCustomer.text = gone.ToString();
    }
    
    public void OnNextButtonClick()
    {
        FadeManager.Instance.FadeOutAndLoadSceneWithLoading("DayScene");
    }
}
