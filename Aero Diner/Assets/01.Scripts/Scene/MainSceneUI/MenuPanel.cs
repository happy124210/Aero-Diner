using DG.Tweening;
using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private GameObject foodItemPrefab;      // 프리팹 연결
    [SerializeField] private Transform contentTransform;      // ScrollView의 Content
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private GameObject warningPopup; // 팝업 루트
    [SerializeField] private CanvasGroup warningPopupCanvas; // 팝업의 CanvasGroup
    [SerializeField] private float popupFadeDuration = 0.5f;
    [SerializeField] private float popupVisibleTime = 2f;

    private void OnEnable()
    {
        GenerateFoodList();
    }

    public void GenerateFoodList()
    {
        // 기존 아이템 제거
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        // 메뉴 가져오기
        var menuList = MenuManager.Instance.GetUnlockedMenus();

        if (menuList == null)
        {
            Debug.LogWarning("TodayMenus 가 null입니다!");
            return;
        }

        Debug.Log($" 메뉴 수: {menuList.Count}");

        foreach (var menu in menuList)
        {
            if (menu == null)
            {
                Debug.LogWarning("null인 Menu 발견");
                continue;
            }
            
            var go = Instantiate(foodItemPrefab, contentTransform);
            var foodUI = go.GetComponent<MenuPanelContent>();
            foodUI.SetData(menu);
        }
        
        EventBus.Raise(UIEventType.ShowMenuPanel);
        EventBus.PlayBGM(BGMEventType.PlayRecipeChoice);
    }
    public void OnClickDayStartBtn()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        // 체크된 토글이 하나라도 있는지 확인
        bool anyToggled = false;

        foreach (Transform child in contentTransform)
        {
            var content = child.GetComponent<MenuPanelContent>();
            if (content != null && content.toggle.isOn)
            {
                anyToggled = true;
                break;
            }
        }

        if (!anyToggled)
        {
            // 아무것도 선택되지 않았을 경우 → 경고 팝업 출력
            Debug.LogWarning("[MenuPanel] 선택된 메뉴가 없습니다!");
            ShowNoMenuSelectedPopup(); // 팝업 메서드 추가 필요
            return;
        }
        // 두트윈으로 페이드 아웃 → 종료 후 비활성화
        canvasGroup.DOFade(0f, 0.5f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                gameObject.SetActive(false); // 비활성화
                EventBus.Raise(UIEventType.HideMenuPanel);
            });
        
        RestaurantManager.Instance.StartRestaurant();
        EventBus.PlayBGM(BGMEventType.PlayMainTheme);
    }

    private void ShowNoMenuSelectedPopup()
    {
        if (warningPopup == null || warningPopupCanvas == null) return;

        warningPopup.SetActive(true);
        warningPopupCanvas.alpha = 0f;
        warningPopupCanvas.DOFade(1f, popupFadeDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(popupVisibleTime, () =>
                {
                    warningPopupCanvas.DOFade(0f, popupFadeDuration)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() => warningPopup.SetActive(false));
                });
            });
    }
}
