using DG.Tweening;
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
    public TextMeshProUGUI TotalRevenueText => totalRevenue;
    [Header("Customer Result")]
    [SerializeField] TextMeshProUGUI allCustomer;
    [SerializeField] TextMeshProUGUI servedCustomer;
    [SerializeField] TextMeshProUGUI goneCustomer;

    [SerializeField] public CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelTransform;
    private Vector2 originalPos;
    private void Awake()
    {
        if (panelTransform == null)
        {
            Debug.LogError("[ResultPanel] panelTransform이 할당되지 않았습니다!");
            return;
        }

        originalPos = panelTransform.anchoredPosition;
        panelTransform.anchoredPosition = originalPos + new Vector2(0, 800f);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
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
        Init(); // 데이터 바인딩 먼저
        StartCoroutine(DelayedAnimate());
    }

    private IEnumerator DelayedAnimate()
    {
        yield return new WaitForSeconds(1f); // 한 프레임 대기
        AnimateEntrance(); // DOTween 애니메이션 실행
    }

    private void SetSalesResult()
    {
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        var salesResults = MenuManager.Instance.GetAllMenuSalesData();
        if (salesResults == null) return;

        float delay = 1.5f;
        foreach (var sales in salesResults)
        {
            var go = Instantiate(contentPrefab, contentTransform);
            var foodUI = go.GetComponent<ResultPanelContent>();
            foodUI.SetData(sales);

            // 애니메이션 연출 추가
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = go.AddComponent<CanvasGroup>();

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition += new Vector2(0, 100f); // 위에서 떨어지기
            cg.alpha = 0f;
            rt.localScale = Vector3.one * 0.8f;

            DOTween.Sequence()
                .AppendInterval(delay)
                .Append(cg.DOFade(1f, 0.3f))
                .Join(rt.DOAnchorPosY(rt.anchoredPosition.y - 100f, 0.3f).SetRelative().SetEase(Ease.OutQuad))
                .Join(rt.DOScale(1f, 0.3f).SetEase(Ease.OutBack));

            delay += 0.05f; // 순차 등장
        }

        totalSalesVolume.text = MenuManager.Instance.GetTotalSalesToday().ToString();
        totalRevenue.text = MenuManager.Instance.GetTotalRevenueToday().ToString();
    }

    private void SetCustomerResult()
    {
        int all = RestaurantManager.Instance.CustomersVisited;
        int served = RestaurantManager.Instance.CustomersServed;
        int gone = all - served;

        float delay = 1.5f;
        AnimateNumber(allCustomer, all, delay);
        delay += 0.3f;

        AnimateNumber(servedCustomer, served, delay);
        delay += 0.3f;

        AnimateNumber(goneCustomer, gone, delay);
    }

    public void OnNextButtonClick()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        EventBus.Raise(UIEventType.HideResultPanel);
        EventBus.RaiseFadeEvent(FadeEventType.FadeOutAndLoadScene, new FadeEventPayload(scene: "DayScene"));
    }
    private void AnimateEntrance()
    {
        if (canvasGroup == null || panelTransform == null) return;

        DG.Tweening.Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1f, 0.7f));
        seq.Join(panelTransform.DOAnchorPos(originalPos, 0.5f).SetEase(Ease.OutBack));
        EventBus.OnBGMRequested(BGMEventType.PlayResultTheme);

    }
    private void AnimateNumber(TextMeshProUGUI targetText, int finalValue, float delay = 0f, float duration = 1.2f)
    {
        DOVirtual.DelayedCall(delay, () =>
        {
            DOVirtual.Int(0, finalValue, duration, value =>
            {
                targetText.text = value.ToString();
            }).SetEase(Ease.OutQuad);
        });
    }
}
