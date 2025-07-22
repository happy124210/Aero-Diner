using UnityEngine;
using System.Linq;

public class StationPanel_ScrollView : MonoBehaviour
{
    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private StationPanel detailPanel;

    private void Start()
    {
        PopulateScrollView();
    }

    private void PopulateScrollView()
    {
        var allStations = Resources.LoadAll<StationData>("Datas/Station")
            .Where(s => !string.IsNullOrEmpty(s.id) && s.id.StartsWith("s"))
            .OrderBy(s =>
            {
                string numericPart = new string(s.id.Where(char.IsDigit).ToArray());
                return int.TryParse(numericPart, out int number) ? number : int.MaxValue;
            })
            .ToList();

        StationPanel_ScrollView_Content firstSlot = null;

        foreach (var station in allStations)
        {
            var go = Instantiate(slotPrefab, contentTransform);
            var slotUI = go.GetComponent<StationPanel_ScrollView_Content>();
            slotUI.Init(station, OnSlotSelected);

            if (firstSlot == null)
                firstSlot = slotUI;
        }

        // 기본 선택
        if (firstSlot != null)
        {
            detailPanel.SetData(firstSlot.GetStationData());
        }
    }

    private void OnSlotSelected(StationData data)
    {
        detailPanel.SetData(data);
        EventBus.PlaySFX(SFXType.ButtonClick);
    }
}
