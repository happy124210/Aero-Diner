using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private TabButtonController tabController;
    public bool IsDebug = false;
    private void Awake()
    {
        if (tabController == null)
            tabController = GetComponentInChildren<TabButtonController>();
    }
    #region 버튼 메서드
    public void OnIngredientTabClick()
    {
        if (IsDebug)
            Debug.Log("버튼 클릭 됨");
        tabController.RequestSelectTab(0);
        EventBus.Raise(UIEventType.ShowInventory);
        tabController.ApplyTabSelectionVisuals();
    }
    public void OnRecipeTabClick()
    {
        if (IsDebug)
            Debug.Log("버튼 클릭 됨");
        tabController.RequestSelectTab(2);
        EventBus.Raise(UIEventType.ShowRecipeBook);
        tabController.ApplyTabSelectionVisuals();
    }
    public void OnStationTabClick()
    {
        if (IsDebug)
            Debug.Log("버튼 클릭 됨");
        tabController.RequestSelectTab(1);
        EventBus.Raise(UIEventType.ShowStationPanel);
        tabController.ApplyTabSelectionVisuals();
    }
    public void OnQuestTabClick()
    {
        if (IsDebug)
            Debug.Log("버튼 클릭 됨");
        tabController.RequestSelectTab(3);
        EventBus.Raise(UIEventType.ShowQuestPanel);
        tabController.ApplyTabSelectionVisuals();
    }
}
#endregion