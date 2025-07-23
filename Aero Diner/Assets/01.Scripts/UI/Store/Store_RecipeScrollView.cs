using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Store_RecipeScrollView : MonoBehaviour
{
    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private GameObject lockedMenuPrefab;
    [SerializeField] private GameObject unlockedMenuPrefab;
    [SerializeField] private Store_RecipePanel detailPanel;
    [SerializeField] private Store store;
    [SerializeField] private GameObject lockedPanel;

    private void Awake()
    {
        if (store == null)
            store = FindObjectOfType<Store>();
    }
    void Start()
    {
        PopulateMenuList();
    }

    public void PopulateMenuList()
    {
        var allMenus = MenuManager.Instance.GetAllMenus()
            .OrderBy(menu => {
                string numPart = new string(menu.foodData.id.Where(char.IsDigit).ToArray());
                return int.TryParse(numPart, out int n) ? n : int.MaxValue;
            }).ToList();

        foreach (var menu in allMenus)
        {
            var prefab = menu.isUnlocked ? unlockedMenuPrefab : lockedMenuPrefab;
            var go = Instantiate(prefab, contentTransform);
            var slot = go.GetComponent<Store_Recipe_Content>();

            slot.Init(menu.foodData, menu.isUnlocked, OnMenuSelected);
        }

        // 첫 해금 메뉴 자동 선택
        var firstUnlocked = allMenus.FirstOrDefault(m => m.isUnlocked);
        if (firstUnlocked != null)
        {
            OnMenuSelected(firstUnlocked.foodData);
        }
    }

    private void OnMenuSelected(FoodData menuData)
    {
        var menu = MenuManager.Instance.FindMenuById(menuData.id);

        EventBus.PlaySFX(SFXType.ButtonClick);

        // 메뉴 정보는 항상 표시
        detailPanel.SetData(menuData, store.TryBuyMenu);

        // 해금 여부에 따라 가림막 토글
        bool isLocked = menu != null && !menu.isUnlocked;
        detailPanel.SetLockedOverlayVisible(isLocked);
    }

}
