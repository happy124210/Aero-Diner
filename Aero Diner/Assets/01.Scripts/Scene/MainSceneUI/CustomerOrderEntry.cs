using UnityEngine;
using UnityEngine.UI;

public class CustomerOrderEntryUI : MonoBehaviour
{
    [SerializeField] private Image foodIcon;
    [SerializeField] private Slider patienceSlider;

    public void Init(Sprite orderSprite)
    {
        foodIcon.sprite = orderSprite;
    }

    public void UpdatePatience(float current, float max)
    {
        float ratio = Mathf.Clamp01(current / max);
        patienceSlider.value = ratio;

        // 색상 조건 변경
        Color fillColor = ratio switch
        {
            > 0.7f => Color.green,
            > 0.3f => Color.yellow,
            _ => Color.red
        };

        // Fill 이미지 색상 변경
        patienceSlider.fillRect.GetComponent<Image>().color = fillColor;
    }
}

