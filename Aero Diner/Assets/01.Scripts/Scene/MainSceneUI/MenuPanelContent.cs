using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuPanelContent : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Toggle toggle;

    private Menu currentMenu;
    
    /// <summary>
    /// Menu 데이터로 UI 설정
    /// </summary>
    public void SetData(Menu menu)
    {
        currentMenu = menu;

        // 기본 UI
        iconImage.sprite = menu.foodData.foodIcon;
        nameText.text = menu.foodData.displayName ?? menu.foodData.foodName;
        priceText.text = $"{menu.foodData.foodCost}G";
        
        // Toggle 기본 상태
        toggle.isOn = menu.isSelected;
    }
    
    public void OnToggle(bool isOn)
    {
        MenuManager.Instance.ToggleMenuSelection(currentMenu.foodData.id);
    }
}
