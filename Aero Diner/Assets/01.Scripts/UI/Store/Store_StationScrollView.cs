using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class Store_StationScrollView : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private Store_StationPanel detailPanel;

    [Header("프리팹")]
    [SerializeField] private GameObject lockedSlotPrefab;
    [SerializeField] private GameObject unlockedSlotPrefab;

    [Header("참조")]
    [SerializeField] private Store store;

    private List<StoreItem> stationStoreItems = new();

    private void Awake()
    {
        if (store == null) store = FindObjectOfType<Store>();
    }

    private void Start()
    {
        InitializeAndPopulate();
    }

    public void InitializeAndPopulate()
    {
        stationStoreItems.Clear();

        var allStations = StationManager.Instance.StationDatabase;
        var storeDataMap = StoreDataManager.Instance.StoreItemMap;

        foreach (var station in allStations)
        {
            if (storeDataMap.TryGetValue(station.Key, out var csvData))
            {
                var storeItem = new StoreItem(station.Value.stationData, csvData);
                stationStoreItems.Add(storeItem);
            }
        }

        stationStoreItems = stationStoreItems.OrderBy(item => {
            string numPart = new string(item.ID.Where(char.IsDigit).ToArray());
            return int.TryParse(numPart, out int n) ? n : int.MaxValue;
        }).ToList();

        PopulateScrollView();
    }

    private void PopulateScrollView()
    {
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in stationStoreItems)
        {
            bool conditionsMet = AreConditionsMet(item);
            GameObject prefabToUse = conditionsMet ? unlockedSlotPrefab : lockedSlotPrefab;

            var go = Instantiate(prefabToUse, contentTransform);
            var slotUI = go.GetComponent<Store_Station_Content>();
            slotUI.Init(item, OnSlotSelected, conditionsMet);
        }

        var firstItem = stationStoreItems.FirstOrDefault();
        if (firstItem != null)
        {
            OnSlotSelected(firstItem);
        }
    }

    private bool AreConditionsMet(StoreItem item)
    {
        if (item.CsvData.Type == UnlockType.None) return true;

        switch (item.CsvData.Type)
        {
            case UnlockType.Quest:
                // TODO: 퀘스트 완료 여부 확인
                return false;
            
            case UnlockType.Recipe:
                return item.CsvData.Conditions.All(id => MenuManager.Instance.FindMenuById(id).isUnlocked);
        }
        return false;
    }

    private void OnSlotSelected(StoreItem item)
    {
        EventBus.PlaySFX(SFXType.ButtonClick);
        bool canBePurchased = AreConditionsMet(item);
        detailPanel.SetData(item, () => store?.TryBuyItem(item), canBePurchased);
    }
}