using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientPanel_ScrollView : MonoBehaviour
{
    public RectTransform contentTransform;
    public GameObject itemSlotPrefab;
    public FoodTypeIconSet iconSet;
    public IngredientPanel ingredientPanel; // 우측 패널

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => RecipeManager.Instance != null && RecipeManager.Instance.FoodDatabase.Count > 0);
        PopulateScrollView();
    }

    private void PopulateScrollView()
    {
        var allFoods = RecipeManager.Instance.FoodDatabase.Values
    .Where(f => !string.IsNullOrEmpty(f.id) && f.id.StartsWith("f"))
    .OrderBy(f =>
    {
        if (int.TryParse(f.id.Substring(1), out int number))
            return number;
        return int.MaxValue; // 실패하면 뒤로 보냄
    })
    .ToList();

        IngredientPanel_ScrollView_Content firstSlotUI = null;

        foreach (var food in allFoods)
        {
            GameObject slot = Instantiate(itemSlotPrefab, contentTransform);
            var slotUI = slot.GetComponent<IngredientPanel_ScrollView_Content>();
            if (slotUI != null)
            {
                slotUI.Init(food, iconSet, ingredientPanel);

                if (firstSlotUI == null)
                    firstSlotUI = slotUI; // 첫 번째 슬롯 저장
            }
        }

        // 첫 슬롯 자동 선택
        if (firstSlotUI != null)
        {
            ingredientPanel.SetData(firstSlotUI.FoodInfo, iconSet.GetIcon(firstSlotUI.FoodInfo.foodType));
        }
    }
}
