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

    public StationData FindStationById(string id) => StationDatabase.GetValueOrDefault(id);

    [System.Serializable]
    public class StationGroup
    {
        public List<GameObject> stations = new List<GameObject>();
    }

    [System.Serializable]
    public class StationSnapshot
    {
        public string prefabName;     // station 프리팹 이름
        public string gridCellName;   // 해당 스테이션이 붙어있던 GridCell 이름
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
    }

    private List<StationData> stationDataList = new List<StationData>();
    private List<Station> stationDatabase = new();                              // 전체 Station타입 스테이션 데이터 담음
    Dictionary<string, (int gridCellCount, int storageGridCellCount)> stationTypeCounts = new();
    private List<StationSnapshot> stationSnapshots = new List<StationSnapshot>();

    public int TotalStationCount;
    public int GridCellStationCount;
    public int StorageGridCellStationCount;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

        InitializeStationDatabase();
    }

    private void Start()
    {
        tilemapController.FindGridCells();
        if (showDebugInfo) Debug.Log($"[StationManager] GridCell 수: {tilemapController.gridCells.Count}");

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
                string stationType = station.name; // 또는 station.GetComponent<Station>().stationData.id 같은 방식

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
        StationData station = FindStationById(id);

        // 상점에서 구매한 스테이션의 정보(스테이션의 아이디)
        // 스테이션 프리팹을 찾아서 생성 - 스테이션 아이디로 프리팹 찾기(스테이션 프리팹은 StationData의 SO데이터를 가지고 있음)
    }

    /// <summary>
    /// 스테이션 상태 저장 함수
    /// </summary>
    public void SaveStationStates()
    {
        stationSnapshots.Clear();

        for (int i = 0; i < stationGroups.Count; i++)
        {
            var group = stationGroups[i];
            var gridCell = tilemapController.gridCells[i];

            foreach (var stationObj in group.stations)
            {
                string prefabName = stationObj.name.Replace("(Clone)", "").Trim();

                stationSnapshots.Add(new StationSnapshot
                {
                    prefabName = prefabName,
                    gridCellName = gridCell.name,
                    localPosition = stationObj.transform.localPosition,
                    localRotation = stationObj.transform.localRotation,
                    localScale = stationObj.transform.localScale
                });

                if (showDebugInfo)
                    Debug.Log($"[Save] 저장: {prefabName} / 위치: {stationObj.transform.localPosition} / 셀: {gridCell.name}");
            }
        }

        if (showDebugInfo)
            Debug.Log($"[StationManager] 상태 저장 완료: {stationSnapshots.Count}개");
    }

    /// <summary>
    /// 기존 Station 제거 함수
    /// </summary>
    public void DestroyAllStations()
    {
        foreach (var group in stationGroups)
        {
            // 씬 오브젝트만 삭제
            foreach (var stationObj in group.stations)
            {
                if (stationObj != null && stationObj.scene.IsValid())
                {
                    Destroy(stationObj);
                }
                else
                {
                    Debug.LogWarning($"[Destroy] {stationObj?.name}은 씬 오브젝트가 아닙니다. 삭제 불가.");
                }
            }

            group.stations.Clear(); // 리스트도 초기화
        }

        FindStations(); // 비운 상태로 갱신
    }

    /// <summary>
    /// Station 복원 함수
    /// </summary>
    public void RestoreStations()
    {
        foreach (var snapshot in stationSnapshots)
        {
            // Resources에서 프리팹 로드
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/{snapshot.prefabName}");
            if (prefab == null)
            {
                Debug.LogWarning($"[Restore] 프리팹 로드 실패: {snapshot.prefabName}");
                continue;
            }

            GameObject gridCell = tilemapController.gridCells
                .FirstOrDefault(cell => cell.name == snapshot.gridCellName);

            if (gridCell == null)
            {
                Debug.LogWarning($"[Restore] 셀 없음: {snapshot.gridCellName}");
                continue;
            }

            // 프리팹 인스턴스 생성 및 셀에 배치
            GameObject instance = Instantiate(prefab);
            instance.transform.SetParent(gridCell.transform, false);

            instance.transform.localPosition = snapshot.localPosition;
            instance.transform.localRotation = snapshot.localRotation;
            instance.transform.localScale = snapshot.localScale;

            if (showDebugInfo)
                Debug.Log($"[Restore] 복원: {instance.name} / 셀: {gridCell.name} / 위치: {instance.transform.localPosition}");
        }

        FindStations();
    }

    private void OnEnable()
    {
        EventBus.Register(GameEventType.GamePhaseChanged, OnGamePhaseChanged);
    }

    private void OnDisable()
    {
        EventBus.Unregister(GameEventType.GamePhaseChanged, OnGamePhaseChanged);
    }

    private GamePhase previousPhase = GamePhase.None;

    private void OnGamePhaseChanged(object phaseObj)
    {
        if (phaseObj is not GamePhase newPhase)
        {
            Debug.LogError("[StationManager] 잘못된 GamePhase 전달");
            return;
        }

        Debug.Log($"[PhaseChange] 이전: {previousPhase}, 새로: {newPhase}");

        // 복원 조건
        if (newPhase == GamePhase.EditStation || newPhase == GamePhase.Day || newPhase == GamePhase.Opening)
        {
            Debug.Log("[PhaseChange] RestoreStations() 호출됨");
            RestoreStations();
        }

        // 저장 & 제거 조건
        if (previousPhase == GamePhase.EditStation || previousPhase == GamePhase.Day || previousPhase == GamePhase.Opening)
        {
            Debug.Log("[PhaseChange] SaveStationStates() 호출됨");
            SaveStationStates();
            DestroyAllStations();
        }

        //// 제거만 조건
        //if (previousPhase == GamePhase.Closing)
        //{
        //    Debug.Log("[PhaseChange] DestroyAllStations() 호출됨");
        //    DestroyAllStations();
        //}

        previousPhase = newPhase;
    }
}
