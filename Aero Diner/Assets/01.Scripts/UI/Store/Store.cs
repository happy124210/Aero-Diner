using DG.Tweening;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Store : MonoBehaviour
{
    [SerializeField] public TabController tabController;
    public bool IsDebug = false;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI curruntMoney;
    [SerializeField] private GameObject insufficientMoneyPanel;
    [SerializeField] private Store_RecipeScrollView recipeScrollView;
    private void Awake()
    {
        if (tabController == null)
            tabController = GetComponentInChildren<TabController>();
        curruntMoney.text = GameManager.Instance.TotalEarnings.ToString();
    }
    private void Update()
    {
        curruntMoney.text = $"{GameManager.Instance.TotalEarnings.ToString()} G";
    }
    public void TryBuyMenu(FoodData data)
    {
        var menu = MenuManager.Instance.FindMenuById(data.id);
        if (menu != null && menu.isUnlocked)
        {
            Debug.LogWarning($"[Store] 이미 해금된 메뉴입니다: {data.displayName}");
            return; // 🔒 중복 구매 차단
        }

        var price = data.foodCost;
        int currentMoney = GameManager.Instance.TotalEarnings;

        if (currentMoney >= price)
        {
            // 돈 차감
            typeof(GameManager).GetMethod("AddMoney", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(GameManager.Instance, new object[] { -price });

            // 해금
            MenuManager.Instance.UnlockMenu(data.id);
            MenuManager.Instance.SaveMenuDatabase();

            // UI 갱신
            recipeScrollView.PopulateMenuList();
        }
        else
        {
            ShowInsufficientMoneyPanel();
        }
    }

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
    #region 두트윈 메서드
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
    public void OnQuestTabClick()
    {
        if (IsDebug)
            Debug.Log("버튼 클릭 됨");
        EventBus.PlaySFX(SFXType.ButtonClick);
        tabController.RequestSelectTab(3);
        //  EventBus.Raise(UIEventType.ShowQuestPanel);
    }
    public void OnCloseButtonClick()
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        EventBus.Raise(UIEventType.FadeOutStore);
    }
}
#endregion
