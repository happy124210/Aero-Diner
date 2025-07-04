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
        salesVolume = transform.FindChild<TextMeshProUGUI>("Tmp_SaleVolume");
        salesIncome = transform.FindChild<TextMeshProUGUI>("Tmp_SaleIncome");
        
        allCustomer = transform.FindChild<TextMeshProUGUI>("Tmp_AllCustomer");
        servedCustomer = transform.FindChild<TextMeshProUGUI>("Tmp_ServedCustomer");
        goneCustomer = transform.FindChild<TextMeshProUGUI>("Tmp_GoneCustomer");
    }

    private void Start()
    {
        allCustomer.text = 
        servedCustomer.text = RestaurantManager.Instance.CustomersServed.ToString();
        
    }
}
