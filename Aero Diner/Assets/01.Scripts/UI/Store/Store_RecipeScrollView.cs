using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Store_RecipeScrollView : MonoBehaviour
{
    [SerializeField] private Store store;
    
    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private GameObject lockedMenuPrefab;
    [SerializeField] private GameObject unlockedMenuPrefab;
    [SerializeField] private Store_RecipePanel detailPanel;
    [SerializeField] private GameObject lockedPanel;

    private List<StoreItem> recipeStoreItems = new ();
    
    private void Awake()
    {
        if (store == null) store = FindObjectOfType<Store>();
    }
    
    void Start()
    {
        InitializeAndPopulate();
    }

    public void InitializeAndPopulate()
    {
        recipeStoreItems.Clear();
        var allMenus = MenuManager.Instance.GetAllMenus();
        var storeDataMap = StoreDataManager.Instance.StoreItemMap;

        // SO 데이터와 CSV 데이터를 조합해서 StoreItem 리스트 생성
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
    
    public void PopulateScrollView()
    {
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in recipeStoreItems)
        {
            // 구매한 레시피 제외
            if (item.IsPurchased) continue;
            
            bool conditionsMet = AreConditionsMet(item);
            GameObject prefabToUse = conditionsMet ? unlockedMenuPrefab : lockedMenuPrefab;

            var go = Instantiate(prefabToUse, contentTransform);
            var slot = go.GetComponent<Store_Recipe_Content>();
            slot.Init(item, selected => OnItemSelected(selected, true));
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
        var menu = MenuManager.Instance.FindMenuById(item.ID);
        return menu != null && menu.isUnlocked;
    }
    
    // 아이템 해금 조건 충족 여부 체크
    public bool AreConditionsMet(StoreItem item)
    {
        // 해금 조건이 없으면 항상 true
        if (item.CsvData.Type == UnlockType.None)
        {
            return true;
        }
        
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
}
