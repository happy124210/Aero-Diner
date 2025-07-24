using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Store_RecipePanel : MonoBehaviour
{
    [SerializeField] private Image menuIcon;
    [SerializeField] private TMP_Text menuNameText;
    [SerializeField] private TMP_Text menuDescriptionText;
    [SerializeField] private TMP_Text saleText;
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject lockedOverlayPanel;
    
    private StoreItem currentItem;

    public void SetData(StoreItem item, Action onBuyClick, bool canBePurchased)
    {
        if (item == null) return;
        currentItem = item;

        menuIcon.sprite = item.Icon;
        menuNameText.text = item.DisplayName;
        saleText.text = $"{item.Cost} G";
        
        menuDescriptionText.text = canBePurchased 
            ? item.Description 
            : StoreDataManager.Instance.GenerateUnlockDescription(item.CsvData);

        buyButton.interactable = canBePurchased;
        SetLockedOverlayVisible(!canBePurchased);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => {
            EventBus.PlaySFX(SFXType.ButtonClick);
            onBuyClick?.Invoke();
        });
    }

    public void SetLockedOverlayVisible(bool isVisible)
    {
        if (lockedOverlayPanel == null) return;

        CanvasGroup group = lockedOverlayPanel.GetComponent<CanvasGroup>();
        if (group == null)
            group = lockedOverlayPanel.AddComponent<CanvasGroup>();

        if (isVisible)
        {
            lockedOverlayPanel.SetActive(true);
            group.alpha = 0f;
            group.DOFade(1f, 0.3f);
        }
        else
        {
            group.DOFade(0f, 0.3f).OnComplete(() => lockedOverlayPanel.SetActive(false));
        }
    }
}
