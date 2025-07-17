using DG.Tweening;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private GameObject foodItemPrefab;      // 프리팹 연결
    [SerializeField] private Transform contentTransform;      // ScrollView의 Content
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private GameObject warningPopup; // 팝업 루트
    [SerializeField] private CanvasGroup warningPopupCanvas; // 팝업의 CanvasGroup
    [SerializeField] private float popupFadeDuration = 0.5f;
    [SerializeField] private float popupVisibleTime = 2f;

    [SerializeField] public RectTransform menuPanelTransform;
    private Vector2 originalPos;
    private void OnEnable()
    {
        GenerateFoodList();

        if (menuPanelTransform != null)
        {
            menuPanelTransform.anchoredPosition = Vector2.zero;
            originalPos = Vector2.zero;

            menuPanelTransform.anchoredPosition += new Vector2(0, 1600f);
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        StartCoroutine(DelayedAnimateEntrance());
    }

    public void GenerateFoodList()
    {
        EventBus.Raise(UIEventType.ShowMenuPanel);
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        var menuList = MenuManager.Instance.GetUnlockedMenus();
        if (menuList == null)
        {
            Debug.LogWarning("TodayMenus 가 null입니다!");
            return;
        }

        Debug.Log($" 메뉴 수: {menuList.Count}");

        float delay = 1f;
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
            #region 메뉴등장 애니메이션
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = go.AddComponent<CanvasGroup>();
            }

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition += new Vector2(0, 100f); // 약간 위에서 떨어짐
            cg.alpha = 0f;
            rt.localScale = Vector3.one * 0.8f;

            DOTween.Sequence()
                .AppendInterval(delay)
                .Append(cg.DOFade(1f, 0.3f))
                .Join(rt.DOAnchorPosY(rt.anchoredPosition.y - 100f, 0.3f).SetRelative().SetEase(Ease.OutQuad))
                .Join(rt.DOScale(1f, 0.3f).SetEase(Ease.OutBack));

            delay += 0.05f; // 순차적으로 나옴
            #endregion
        }
    }

    public void OnClickDayStartBtn()
    {
        _ = HandleDayStartAsync();
    }

    private async Task HandleDayStartAsync()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);

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
            Debug.LogWarning("[MenuPanel] 선택된 메뉴가 없습니다!");
            ShowNoMenuSelectedPopup();
            return;
        }

        if (menuPanelTransform == null)
        {
            Debug.LogError("[MenuPanel] menuPanelTransform이 할당되지 않았습니다.");
            return;
        }

        PlayExitAnimation();
        //이거 위치 UI/Mainscene/Fader.cs(Start)로 옮겼습니다.
        //RestaurantManager.Instance.StartRestaurant();
        //EventBus.PlayBGM(BGMEventType.PlayMainTheme);

    }


    #region 애니메이션

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
    private IEnumerator DelayedAnimateEntrance()
    {
        yield return new WaitForSeconds(0.5f); // 약간의 연출 지연
        AnimateEntrance();
    }
    private void AnimateEntrance()
    {
        if (canvasGroup == null || menuPanelTransform == null) return;

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);
        seq.Append(canvasGroup.DOFade(1f, 0.7f));
        seq.Join(menuPanelTransform.DOAnchorPos(originalPos, 0.5f).SetEase(Ease.OutBack));
    }
    private void PlayExitAnimation()
    {
        Vector2 originalPos = menuPanelTransform.anchoredPosition;

        Sequence exitSeq = DOTween.Sequence();
        exitSeq.Append(menuPanelTransform.DOAnchorPosY(originalPos.y - 50f, 0.3f).SetEase(Ease.InQuad))
               .Append(menuPanelTransform.DOAnchorPosY(originalPos.y + 1200f, 1f).SetEase(Ease.InBack))
               .OnComplete(() =>
               {
                   gameObject.SetActive(false);
                   menuPanelTransform.anchoredPosition = originalPos;

                   EventBus.Raise(UIEventType.HideMenuPanel);
                   EventBus.RaiseFadeEvent(FadeEventType.FadeOutAndLoadScene, new FadeEventPayload(1f, 1f, scene: "MainScene"));
               });
    }
}
#endregion
