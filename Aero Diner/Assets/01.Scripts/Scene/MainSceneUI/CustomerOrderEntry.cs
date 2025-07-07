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
        patienceSlider.value = Mathf.Clamp01(current / max);
        // 색상 변경도 원하면 여기서 처리 가능
    }
}

