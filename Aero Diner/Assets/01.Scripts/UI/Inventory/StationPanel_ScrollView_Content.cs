using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StationPanel_ScrollView_Content : MonoBehaviour
{
    [SerializeField] private Image stationIconImage;
    [SerializeField] private TMP_Text stationNameText;
    [SerializeField] private Button selectButton;

    private StationData myData;

    public StationData GetStationData() => myData;
    public void Init(StationData data, Action<StationData> onClick)
    {
        myData = data;
        stationNameText.text = data.displayName;
        stationIconImage.sprite = data.stationIcon;

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onClick?.Invoke(myData));
    }
}
