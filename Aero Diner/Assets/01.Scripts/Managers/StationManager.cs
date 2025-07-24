using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StationManager : Singleton<StationManager>
{
    public Dictionary<string, StationData> StationDatabase { get; private set; } = new (); // 전체 Station타입 스테이션 데이터 담음
    public List<StationGroup> stationGroups = new List<StationGroup>();         // 각 GridCell 아래에 있는 Station 리스트

    [SerializeField] private TilemapController tilemapController;
    [SerializeField] private List<GameObject> stationPrefabs = new();

    [Header("디버깅")]
    [SerializeField] private bool showDebugInfo;
    
    [System.Serializable]
    public class StationGroup
    {
        public List<GameObject> stations = new List<GameObject>();
    }

    public int TotalStationCount;
    public int GridCellStationCount;
    public int StorageGridCellStationCount;

    Dictionary<string, (int gridCellCount, int storageGridCellCount)> stationTypeCounts = new();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

        InitializeStationDatabase();
    }

    private void Start()
    {
        tilemapController.FindGridCells();
        if (showDebugInfo)Debug.Log($"[StationManager] GridCell 수: {tilemapController.gridCells.Count}");

        FindStations();
        CountStationsPerCellType();
    }

    /// <summary>
    /// GridCell 아래의 IMovableStation 오브젝트들을 수집해서 구조화
    /// </summary>
    public void FindStations()
    {
        if (tilemapController == null)
        {
            Debug.LogError("[StationManager] tilemapController가 할당되지 않았습니다.");
            return;
        }

        stationGroups.Clear();

        foreach (GameObject gridCell in tilemapController.gridCells)
        {
            StationGroup group = new StationGroup();

            foreach (Transform child in gridCell.transform)
            {
                if (child.TryGetComponent<IMovableStation>(out _))
                {
                    group.stations.Add(child.gameObject);
                }
            }

            stationGroups.Add(group);
        }

        if (showDebugInfo) Debug.Log($"[StationManager] GridCell {stationGroups.Count}개에서 스테이션을 수집했습니다.");
    }

    /// <summary>
    /// 스테이션 데이터베이스 초기화
    /// </summary>
    private void InitializeStationDatabase()
    {
        StationData[] allStations = Resources.LoadAll<StationData>(StringPath.STATION_DATA_PATH);
    
        StationDatabase.Clear();
        foreach (var station in allStations)
        {
            if (!string.IsNullOrEmpty(station.id))
            {
                StationDatabase[station.id] = station;
            }
        }

        if (showDebugInfo) Debug.Log($"StationManager]: 전체 {StationDatabase.Count}개  데이터베이스 생성 완료");
    }

    /// <summary>
    /// 각 GridCell 타입별로 스테이션 개수를 세고 출력
    /// </summary>
    public void CountStationsPerCellType()
    {
        if (tilemapController == null || stationGroups == null || tilemapController.gridCells.Count != stationGroups.Count)
        {
            if (showDebugInfo) Debug.LogError("[StationManager] 데이터 정합성 오류: gridCell 개수와 stationGroup 개수가 맞지 않습니다.");
            return;
        }

        TotalStationCount = 0;
        GridCellStationCount = 0;
        StorageGridCellStationCount = 0;

        Dictionary<string, (int gridCellCount, int storageGridCellCount)> stationTypeCounts = new();

        for (int i = 0; i < stationGroups.Count; i++)
        {
            GameObject gridCell = tilemapController.gridCells[i];
            StationGroup group = stationGroups[i];

            string cellType = gridCell.name.StartsWith("StorageGridCell") ? "Storage" : "Grid";

            foreach (var station in group.stations)
            {
                string stationType = station.name;

                // 스테이션 개수 카운트
                TotalStationCount++;

                if (cellType == "Grid") GridCellStationCount++;
                else if (cellType == "Storage") StorageGridCellStationCount++;

                if (!stationTypeCounts.ContainsKey(stationType))
                {
                    stationTypeCounts[stationType] = (0, 0);
                }

                var counts = stationTypeCounts[stationType];

                if (cellType == "Grid")
                    stationTypeCounts[stationType] = (counts.gridCellCount + 1, counts.storageGridCellCount);
                else
                    stationTypeCounts[stationType] = (counts.gridCellCount, counts.storageGridCellCount + 1);
            }
        }

        // 결과 출력
        foreach (var kvp in stationTypeCounts)
        {
            string stationName = kvp.Key;
            var (gridCount, storageCount) = kvp.Value;
            Debug.Log($"[StationManager] {stationName} → GridCell: {gridCount}, StorageGridCell: {storageCount}");
        }

        Debug.Log($"[StationManager] 전체 스테이션 수: {TotalStationCount} (GridCell: {GridCellStationCount}, StorageGridCell: {StorageGridCellStationCount})");
    }

    /// <summary>
    /// 보관장소에 스테이션 생성
    /// </summary>
    private void CreateStationInStorage(string id)
    {
        // 상점에서 구매한 스테이션의 정보(스테이션의 아이디)
        // 스테이션 프리팹을 찾아서 생성 - 스테이션 아이디로 프리팹 찾기(스테이션 프리팹은 StationData의 SO데이터를 가지고 있음)
    }
}
