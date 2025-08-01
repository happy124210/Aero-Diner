using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Store_RecipeScrollView : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private Store_RecipePanel detailPanel;
    [SerializeField] private GameObject lockedPanel;
    [SerializeField] private GameObject noRecipePanel;
    
    [Header("프리팹")]
    [SerializeField] private GameObject lockedMenuPrefab;
    [SerializeField] private GameObject unlockedMenuPrefab;
    
    [Header("참조")]
    [SerializeField] private Store store;
    
    private List<StoreItem> recipeStoreItems = new ();
    
    private void Awake()
    {
        if (store == null) store = FindObjectOfType<Store>();
    }
    
    private void Start()
    {
        InitializeAndPopulate();
    }

    public void InitializeAndPopulate()
    {
        recipeStoreItems.Clear();
        var allMenus = MenuManager.Instance.GetAllMenus();
        var storeDataMap = StoreDataManager.Instance.StoreItemMap;
        
        foreach (var menu in allMenus)
        {
            if (storeDataMap.TryGetValue(menu.foodData.id, out var csvData))
            {
                var storeItem = new StoreItem(menu.foodData, csvData);
                storeItem.IsPurchased = IsAlreadyPurchased(storeItem);
                recipeStoreItems.Add(storeItem);
            }
        }
        
        // 정렬
        recipeStoreItems = recipeStoreItems.OrderBy(item => {
            string numPart = new string(item.ID.Where(char.IsDigit).ToArray());
            return int.TryParse(numPart, out int n) ? n : int.MaxValue;
        }).ToList();
        
        PopulateScrollView();
    }

    private void PopulateScrollView()
    {
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        bool hasAvailableItem = false;

        foreach (var item in recipeStoreItems)
        {
            if (item.IsPurchased) continue;

            bool conditionsMet = AreConditionsMet(item);
            GameObject prefabToUse = conditionsMet ? unlockedMenuPrefab : lockedMenuPrefab;

            var go = Instantiate(prefabToUse, contentTransform);
            var slot = go.GetComponent<Store_Recipe_Content>();
            slot.Init(item, selected => OnItemSelected(selected, true));

            hasAvailableItem = true; // 하나라도 추가되면 true
        }

        // 조건: 판매 가능한 레시피가 없을 때
        if (!hasAvailableItem)
        {
            if (detailPanel != null) detailPanel.gameObject.SetActive(false);
            if (noRecipePanel != null) noRecipePanel.SetActive(true);
            return;
        }
        else
        {
            if (detailPanel != null) detailPanel.gameObject.SetActive(true);
            if (noRecipePanel != null) noRecipePanel.SetActive(false);
        }

        // 첫 번째 아이템 자동 선택
        var firstItem = recipeStoreItems.FirstOrDefault(i => !i.IsPurchased);
        if (firstItem != null)
        {
            OnItemSelected(firstItem, false);
        }
    }

    // 구매했는지 체크
    private bool IsAlreadyPurchased(StoreItem item)
    {
        var menu = MenuManager.Instance.FindMenuById(item.TargetID);
        return menu != null && menu.isUnlocked;
    }
    
    // 아이템 해금 조건 충족 여부 체크
    private bool AreConditionsMet(StoreItem item)
    {
        // 해금 조건이 없으면 항상 true
        if (item.CsvData.Type == UnlockType.None) return true;
        
        switch (item.CsvData.Type)
        {
            case UnlockType.Quest:
                // TODO : 퀘스트 체크
                return true;
            case UnlockType.Recipe:
                return item.CsvData.Conditions.All(recipeId => MenuManager.Instance.FindMenuById(recipeId).isUnlocked);
        }

        return false;
    }
    
    // 아이템 선택 시 상세 정보 패널 업데이트
    private void OnItemSelected(StoreItem item, bool playSFX = true)
    {
        if (playSFX)
            EventBus.PlaySFX(SFXType.ButtonClick);

        bool canBePurchased = AreConditionsMet(item);
        detailPanel.SetData(item, () => store.TryBuyItem(item), canBePurchased);
    }
    
    
    /// <summary>
    /// 지정된 ID의 레시피 아이템을 강제로 구매 완료 상태로 만듦 (튜토리얼용)
    /// </summary>
    public void ForcePurchaseItem(string id)
    {
        var itemToPurchase = recipeStoreItems.FirstOrDefault(item => item.ID == id);
        
        if (itemToPurchase != null && !itemToPurchase.IsPurchased)
        {
            itemToPurchase.IsPurchased = true;
            PopulateScrollView();
            
            //Debug.Log($"[Store_RecipeScrollView] 아이템 강제 구매 처리 완료: {id}");
        }
        else if (itemToPurchase == null)
        {
            Debug.LogWarning($"[Store_RecipeScrollView] ID '{id}'에 해당하는 아이템을 찾을 수 없습니다.");
        }
    }
}
