using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Store_Recipe_Content : BaseScrollViewItem
{
    [SerializeField] private TMP_Text menuNameText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image resultIconImage;

    private FoodData menuData;
    public void Init(FoodData data, bool isUnlocked, Action<FoodData> onClick)
    {
        menuData = data;

        menuNameText.text = data.displayName;
        resultIconImage.sprite = data.foodIcon;

        selectButton.interactable = true;
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onClick?.Invoke(menuData));

    }
}
