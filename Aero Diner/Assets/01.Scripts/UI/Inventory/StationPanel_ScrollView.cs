using UnityEngine;
using System.Linq;

public class StationPanel_ScrollView : MonoBehaviour
{
    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private StationPanel detailPanel;
    [SerializeField] private GameObject noItemPanel;
    [SerializeField] private GameObject unlockedSlotPrefab;
    [SerializeField] private GameObject lockedSlotPrefab;
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

        if (allStations.Count == 0)
        {
            detailPanel.gameObject.SetActive(false);
            if (noItemPanel != null)
                noItemPanel.SetActive(true);
            return;
        }

        detailPanel.gameObject.SetActive(true);
        if (noItemPanel != null)
            noItemPanel.SetActive(false);

        StationPanel_ScrollView_Content firstUnlocked = null;

        foreach (var station in allStations)
        {
            bool isUnlocked = StationManager.Instance.IsUnlocked(station.id);

            GameObject prefabToUse = isUnlocked ? unlockedSlotPrefab : lockedSlotPrefab;
            var go = Instantiate(prefabToUse, contentTransform);

            if (isUnlocked)
            {
                var slotUI = go.GetComponent<StationPanel_ScrollView_Content>();
                slotUI.Init(station, OnSlotSelected);

                if (firstUnlocked == null)
                    firstUnlocked = slotUI;
            }
            else
            {
                var slotUI = go.GetComponent<StationLockedItemUI>();
                slotUI.Init(station);
            }
        }

        if (firstUnlocked != null)
        {
            detailPanel.SetData(firstUnlocked.GetStationData());
        }
    }

    private void OnSlotSelected(StationData data)
    {
        detailPanel.SetData(data);
        EventBus.PlaySFX(SFXType.ButtonClick);
    }
}
