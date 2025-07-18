using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestPanel : MonoBehaviour
{
    [SerializeField] private TabButtonController tabController;
    [SerializeField] private GameObject doingScrollView;
    [SerializeField] private GameObject completeScrollView;
    public bool IsDebug = false;

     private void Awake()
    {
        if (tabController == null)
            tabController = GetComponentInChildren<TabButtonController>();
    }

    private void OnEnable()
    {
        doingScrollView.SetActive(true);
        completeScrollView.SetActive(false);
        tabController.RequestSelectTab(0);
        tabController.ApplyTabSelectionVisuals();
    }
    #region 버튼메서드
    public void OnClickDoningBtn()
    {
        if (IsDebug)
            Debug.Log("버튼 클릭 됨");
        EventBus.PlaySFX(SFXType.ButtonClick);
        tabController.RequestSelectTab(0);
        doingScrollView.SetActive(true);
        completeScrollView.SetActive(false);
        tabController.ApplyTabSelectionVisuals();
    }
    public void OnClickCompleteBtn()
    {
        if (IsDebug)
            Debug.Log("버튼 클릭 됨");
        EventBus.PlaySFX(SFXType.ButtonClick);
        tabController.RequestSelectTab(1);
        doingScrollView.SetActive(false);
        completeScrollView.SetActive(true);
        tabController.ApplyTabSelectionVisuals();
    }
}
#endregion