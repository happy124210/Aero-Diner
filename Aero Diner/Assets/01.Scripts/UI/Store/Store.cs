using DG.Tweening;
using TMPro;
using UnityEngine;

public class Store : MonoBehaviour
{
    [SerializeField] private TabController tabController;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI currentMoney;
    [SerializeField] private GameObject insufficientMoneyPanel;
    [SerializeField] private Store_RecipeScrollView recipeScrollView;
    private int currentDisplayAmount;
    private Color originalColor;
    [SerializeField] private float animateDuration = 0.5f;
    [SerializeField] private Color flashColor = Color.yellow;
    //TODO: StationScrollView

    [Header("Debug Info")]
    [SerializeField] private bool IsDebug = false;

    private void Awake()
    {
        if (tabController == null)
            tabController = GetComponentInChildren<TabController>();

        currentDisplayAmount = GameManager.Instance.TotalEarnings;
        currentMoney.text = $"{currentDisplayAmount:N0} G";
        originalColor = currentMoney.color;
    }

    public void TryBuyItem(StoreItem item)
    {
        if (item == null || item.IsPurchased) return;
        
        if (GameManager.Instance.TotalEarnings >= item.Cost)
        {
            GameManager.Instance.AddMoney(-item.Cost);
            //재화 업데이트 애니메이션
            AnimateStoreMoney(GameManager.Instance.TotalEarnings);
            EventBus.Raise(UIEventType.UpdateTotalEarnings, GameManager.Instance.TotalEarnings);
            
            switch (item.BaseData)
            {
                // 레시피
                case FoodData:
                    MenuManager.Instance.UnlockMenu(item.ID);
                    break;
                
                // 설비
                case StationData:
                    // TODO: 설비 추가 로직
                    break;
            }
            
            item.IsPurchased = true;
            
            Debug.Log($"구매 성공: {item.DisplayName}");
            recipeScrollView.InitializeAndPopulate();
            
            // TODO: stationScrollView.PopulateScrollView();
        }
        else
        {
            ShowInsufficientMoneyPanel();
        }
    }
    
    #region 두트윈 메서드
    
    private void ShowInsufficientMoneyPanel()
    {
        var group = insufficientMoneyPanel.GetComponent<CanvasGroup>();
        if (group == null)
            group = insufficientMoneyPanel.AddComponent<CanvasGroup>();

        group.alpha = 0;
        insufficientMoneyPanel.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(group.DOFade(1, 0.5f))
            .AppendInterval(1.2f)
            .Append(group.DOFade(0, 0.5f))
            .OnComplete(() => insufficientMoneyPanel.SetActive(false));
    }
    public void AnimateStoreMoney(int newAmount)
    {
        DOTween.Kill(currentMoney);
        DOTween.Kill(currentMoney.transform);

        int fromAmount = currentDisplayAmount;

        DOVirtual.Int(fromAmount, newAmount, animateDuration, value =>
        {
            currentDisplayAmount = value;
            currentMoney.text = $"{value:N0} G";
        }).SetEase(Ease.OutCubic);

        var seq = DOTween.Sequence();
        seq.Append(currentMoney.DOColor(flashColor, 0.2f));
        seq.Join(currentMoney.transform.DOScale(1.2f, 0.2f));
        seq.AppendInterval(0.1f);
        seq.Append(currentMoney.DOColor(originalColor, 0.2f));
        seq.Join(currentMoney.transform.DOScale(1.0f, 0.2f));
    }
    public void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad);
    }
    
    public void Hide()
    {
        canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        });
    }
    #endregion
    
    #region 버튼 메서드
    public void OnIngredientTabClick()
    {
        if (IsDebug)
            Debug.Log("버튼 클릭 됨");
        EventBus.PlaySFX(SFXType.ButtonClick);
        tabController.RequestSelectTab(0);
        // EventBus.Raise(UIEventType.ShowInventory);
        // TODO: 아직 해금 안 됨 경고 팝업
    }
    
    public void OnRecipeTabClick()
    {
        if (IsDebug)
            Debug.Log("버튼 클릭 됨");
        EventBus.PlaySFX(SFXType.ButtonClick);
        tabController.RequestSelectTab(1);
        // EventBus.Raise(UIEventType.ShowRecipeBook);
    }
    
    public void OnStationTabClick()
    {
        if (IsDebug)
            Debug.Log("버튼 클릭 됨");
        EventBus.PlaySFX(SFXType.ButtonClick);
        tabController.RequestSelectTab(0);
        // EventBus.Raise(UIEventType.ShowStationPanel);
    }
    
    public void OnCloseButtonClick()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        EventBus.Raise(UIEventType.FadeOutStore);
    }
    
    #endregion
}