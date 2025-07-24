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
    //TODO: StationScrollView
    
    [Header("Debug Info")]
    [SerializeField] private bool IsDebug = false;
    
    private void Awake()
    {
        if (tabController == null)
            tabController = GetComponentInChildren<TabController>();
        currentMoney.text = GameManager.Instance.TotalEarnings.ToString();
    }
    
    private void Update()
    {
        currentMoney.text = $"{GameManager.Instance.TotalEarnings.ToString()} G";
    }
    
    public void TryBuyItem(StoreItem item)
    {
        if (item == null) return;

        if (item.IsUnlocked)
        {
            Debug.LogWarning($"[Store] 이미 해금된 아이템: {item.DisplayName}");
            return;
        }
        
        if (GameManager.Instance.TotalEarnings >= item.Cost)
        {
            GameManager.Instance.AddMoney(-item.Cost);

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
            
            Debug.Log($"구매 성공: {item.DisplayName}");
            
            recipeScrollView.PopulateScrollView();
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