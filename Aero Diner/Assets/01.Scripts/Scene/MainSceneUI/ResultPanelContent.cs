using System;
using UnityEngine;
using TMPro;

public class ResultPanelContent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI menuName;
    [SerializeField] private TextMeshProUGUI soldCount;
    [SerializeField] private TextMeshProUGUI menuRevenue;

    private Menu currentMenu;

    private void Reset()
    {
        menuName = transform.FindChild<TextMeshProUGUI>("Tmp_MenuName");
        soldCount = transform.FindChild<TextMeshProUGUI>("Tmp_SoldCount");
        menuRevenue = transform.FindChild<TextMeshProUGUI>("Tmp_MenuRevenue");
    }

    /// <summary>
    /// Menu 데이터로 UI 설정
    /// </summary>
    public void SetData(Menu menu)
    {
        currentMenu = menu;
        int sold = MenuManager.Instance.GetTodayMenuSales(menu.foodData.id);
        
        menuName.text = menu.foodData.displayName;
        soldCount.text = sold.ToString();
        menuRevenue.text = (sold * menu.Price).ToString();
    }
}
