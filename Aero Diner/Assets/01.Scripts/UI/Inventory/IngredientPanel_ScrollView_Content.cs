using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FoodSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image typeIconImage;
    [SerializeField] private Button detailButton; // 버튼 참조

    private FoodData myData;
    [SerializeField] private FoodTypeIconSet iconSet;
    [SerializeField] private IngredientPanel detailPanel;
    public FoodData FoodInfo => myData;

    public void Init(FoodData data, FoodTypeIconSet iconSet, IngredientPanel panel)
    {
        myData = data;
        this.iconSet = iconSet;
        detailPanel = panel;

        // 디버깅 출력
        Debug.Log($"[FoodSlotUI] Init: {data.displayName}, icon = {(data.foodIcon != null ? data.foodIcon.name : "NULL")}");

        // UI 표시
        if (iconImage != null)
            iconImage.sprite = data.foodIcon;
        else
            Debug.LogWarning("iconImage is not assigned!");

        nameText.text = data.displayName;


        if (typeIconImage != null && iconSet != null)
            typeIconImage.sprite = iconSet.GetIcon(data.foodType);

        if (detailButton != null)
        {
            detailButton.onClick.RemoveAllListeners();
            detailButton.onClick.AddListener(() =>
            {
                detailPanel.SetData(myData, iconSet.GetIcon(myData.foodType));
            });
        }
    }
}
