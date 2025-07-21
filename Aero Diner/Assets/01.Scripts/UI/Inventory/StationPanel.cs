using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StationPanel : MonoBehaviour
{
    [SerializeField] private Image stationIconImage;
    [SerializeField] private TMP_Text stationNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;

    public void SetData(StationData data)
    {
        stationIconImage.sprite = data.stationIcon;
        stationNameText.text = data.displayName;
        descriptionText.text = data.description;
        costText.text = $"{data.stationCost} G";
    }
}
