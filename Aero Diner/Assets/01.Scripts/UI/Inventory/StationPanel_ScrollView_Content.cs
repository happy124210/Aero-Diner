using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StationPanel_ScrollView_Content : BaseScrollViewItem
{
    [SerializeField] private Image stationIconImage;
    [SerializeField] private TMP_Text stationNameText;
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text placedCountText;
    [SerializeField] private TMP_Text storedCountText;

    private string stationcost;

    private StationData myData;

    public StationData GetStationData() => myData;
    public void Init(StationData data, Action<StationData> onClick)
    {
        myData = data;

        stationNameText.text = data.displayName;
        stationIconImage.sprite = data.stationIcon;

        int placed = StationManager.Instance.GetPlacedStationCount(data.id);
        int stored = StationManager.Instance.GetStoredStationCount(data.id);

        placedCountText.text = $"배치됨: {placed}";
        storedCountText.text = $"보관됨: {stored}";

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onClick?.Invoke(myData));
    }
}
