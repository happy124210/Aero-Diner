using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class RecipePanel_RecipeScrollView : MonoBehaviour
{
    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private GameObject menuSlotPrefab;
    [SerializeField] private RecipePanel_IngredientScrollView ingredientScroll;
    [SerializeField] private RecipePanel detailPanel;
    [SerializeField] private GameObject noItemPanel;
    private FoodData currentSelected;
    private bool suppressNextSFX = false;
    void Start()
    {
        PopulateMenuList();
    }

    private void PopulateMenuList()
    {
        var menus = MenuManager.Instance.GetUnlockedMenus()
            .OrderBy(menu =>
            {
                string numPart = new string(menu.foodData.id.Where(char.IsDigit).ToArray());
                return int.TryParse(numPart, out int n) ? n : int.MaxValue;
            })
            .Select(menu => menu.foodData)
            .ToList();

        // 아무 레시피도 없음
        if (!menus.Any())
        {
            detailPanel.gameObject.SetActive(false);
            noItemPanel.SetActive(true);
            return;
        }

        // 레시피 있음
        detailPanel.gameObject.SetActive(true);
        noItemPanel.SetActive(false);

        foreach (var menu in menus)
        {
            var go = Instantiate(menuSlotPrefab, contentTransform);
            var slot = go.GetComponent<RecipePanel_RecipeScrollView_Content>();
            slot.Init(menu, OnSlotSelected);
        }

        if (menus.Count > 0)
        {
            suppressNextSFX = true;
            OnSlotSelected(menus[0]);
        }
    }

    private void OnSlotSelected(FoodData menuData)
    {
        currentSelected = menuData; // 계속 갱신해도 괜찮음

        if (!suppressNextSFX)
            EventBus.PlaySFX(SFXType.ButtonClick);
        else
            suppressNextSFX = false;

        ingredientScroll.PopulateIngredients(menuData);
        detailPanel.SetData(menuData);
    }
}

