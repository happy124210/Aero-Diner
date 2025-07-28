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
        PopulateStationList();
    }

    private void PopulateStationList()
    {
        // 기존 UI 슬롯들 모두 삭제
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
        
        var allStations = StationManager.Instance.GetAllStations()
            .OrderBy(s =>
            {
                string numPart = new string(s.stationData.id.Where(char.IsDigit).ToArray());
                return int.TryParse(numPart, out int n) ? n : int.MaxValue;
            })
            .ToList();

        // 아무 설비도 없는 경우
        if (!allStations.Any())
        {
            detailPanel.gameObject.SetActive(false);
            noItemPanel?.SetActive(true);
            return;
        }
        
        // 설비 있음
        detailPanel.gameObject.SetActive(true);
        noItemPanel?.SetActive(false);

        StationPanel_ScrollView_Content firstUnlockedSlot = null;

        // 모든 스테이션을 순회하며 해금/잠금 상태에 맞는 UI 생성
        foreach (var station in allStations)
        {
            GameObject prefabToUse = station.isUnlocked ? unlockedSlotPrefab : lockedSlotPrefab;
            GameObject go = Instantiate(prefabToUse, contentTransform);

            if (station.isUnlocked)
            {
                // 해금된 슬롯 초기화
                var slotUI = go.GetComponent<StationPanel_ScrollView_Content>();
                slotUI.Init(station.stationData, OnSlotSelected);
                
                if (firstUnlockedSlot == null)
                {
                    firstUnlockedSlot = slotUI;
                }
            }
            else
            {
                // 잠긴 슬롯 초기화
                var slotUI = go.GetComponent<StationLockedItemUI>();
                slotUI.Init(station.stationData);
            }
        }
        
        if (firstUnlockedSlot != null)
        {
            detailPanel.SetData(firstUnlockedSlot.GetStationData());
        }
        else
        {
            detailPanel.gameObject.SetActive(false);
        }
    }
    
    private void OnSlotSelected(StationData data)
    {
        detailPanel.SetData(data);
        EventBus.PlaySFX(SFXType.ButtonClick);
    }
}
