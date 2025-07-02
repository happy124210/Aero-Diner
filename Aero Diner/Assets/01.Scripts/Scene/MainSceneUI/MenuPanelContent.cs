using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuPanelContent : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text priceText;

    public void SetData(FoodData foodData)
    {
        iconImage.sprite = foodData.foodIcon;
        nameText.text = foodData.foodName;
        priceText.text = $"{foodData.foodCost}G";
    }
}
