using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultPanel : MonoBehaviour
{
    [Header("Sales")]
    // TODO: 콘텐츠 패널
    [SerializeField] TextMeshProUGUI salesVolume;
    [SerializeField] TextMeshProUGUI salesIncome;
    
    [Header("Customer Result")]
    [SerializeField] TextMeshProUGUI allCustomer;
    [SerializeField] TextMeshProUGUI servedCustomer;
    [SerializeField] TextMeshProUGUI goneCustomer;

    private void Reset()
    {
        salesVolume = transform.FindChild<TextMeshProUGUI>("Tmp_SalesVolume");
        salesIncome = transform.FindChild<TextMeshProUGUI>("Tmp_SalesIncome");
        
        allCustomer = transform.FindChild<TextMeshProUGUI>("Tmp_AllCustomer");
        servedCustomer = transform.FindChild<TextMeshProUGUI>("Tmp_ServedCustomer");
        goneCustomer = transform.FindChild<TextMeshProUGUI>("Tmp_GoneCustomer");
    }

    private void OnEnable()
    {
        int all = RestaurantManager.Instance.CustomersVisited;
        int served = RestaurantManager.Instance.CustomersServed;
        int gone = all - served;
        
        allCustomer.text = all.ToString();
        servedCustomer.text = served.ToString();
        goneCustomer.text = gone.ToString();
    }
}
