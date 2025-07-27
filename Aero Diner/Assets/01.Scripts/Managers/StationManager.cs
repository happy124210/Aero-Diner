using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SaveData;

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
        public GameObject station;
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
    public static new StationManager Instance;

    protected override void Awake()
    {
        tilemapController = FindObjectOfType<TilemapController>();

        base.Awake();
        DontDestroyOnLoad(this);

        InitializeStationDatabase();
    }

    private void Start()
    {
        tilemapController.FindGridCells();
        if (showDebugInfo) Debug.Log($"[StationManager] GridCell 수: {tilemapController.gridCells.Count}");

        SetStations();
        CountStationsPerCellType();
    }

    public void SetTilemapController(TilemapController controller)
    {
        tilemapController = controller;
        Debug.Log("[StationManager] 타일맵 컨트롤러 연결 완료");
    }


    /// <summary>
    /// GridCell 아래의 오브젝트들을 수집해서 구조화
    /// </summary>
    private void SetStations()
    {
        if (tilemapController == null)
        {
            Debug.LogError("[StationManager] tilemapController가 할당되지 않았습니다.");
            return;
        }

        stationGroups.Clear();

        for (int i = 0; i < tilemapController.gridCells.Count; i++)
        {
            GameObject gridCell = tilemapController.gridCells[i];
            StationGroup group = new StationGroup();

            foreach (Transform child in gridCell.transform)
            {
                if (child.TryGetComponent<IMovableStation>(out _))
                {
                    group.station = child.gameObject;
                    break;
                }
            }

            stationGroups.Add(group);

            // 디버그 로그 추가: 셀 이름과 스테이션 이름 출력
            string cellName = gridCell.name;
            string stationName = group.station != null ? group.station.name : "없음";
            Debug.Log($"[StationManager] '{cellName}' 셀에 스테이션: {stationName}");
        }

        Debug.Log($"[StationManager] 총 {stationGroups.Count}개의 GridCell에서 스테이션을 수집했습니다.");
    }

    public List<StationSaveInfo> GenerateStationSaveData()
    {
        var saveList = new List<StationSaveInfo>();

        for (int i = 0; i < stationGroups.Count; i++)
        {
            var group = stationGroups[i];
            var cell = tilemapController.gridCells[i];

            if (group.station == null)
            {
                Debug.LogWarning($"[Generate] stationGroups[{i}]의 station이 null → 저장 제외됨 (cell: '{cell.name}')");
                continue;
            }

            var info = new StationSaveInfo
            {
                id = group.station.name, // 혹은 station.GetComponent<Station>().stationData.id
                gridCellName = cell.name
            };

            saveList.Add(info);
            Debug.Log($"[Generate] 저장됨: id = '{info.id}', cell = '{info.gridCellName}'");
        }

        return saveList;
    }

    public void RestoreStations(List<StationSaveInfo> infos)
    {
        // stationGroups 초기화
        stationGroups.Clear();
        for (int i = 0; i < tilemapController.gridCells.Count; i++)
            stationGroups.Add(new StationGroup());

        foreach (var info in infos)
        {
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/Stations/{info.id}");
            if (prefab == null)
            {
                Debug.LogError($"[Restore] 프리팹 못 찾음: {info.id}");
                continue;
            }

            GameObject cell = tilemapController.gridCells.FirstOrDefault(c => c.name == info.gridCellName);
            if (cell == null)
            {
                Debug.LogError($"[Restore] 셀 못 찾음: {info.gridCellName}");
                continue;
            }

            GameObject instance = Instantiate(prefab, cell.transform);
            instance.transform.localPosition = Vector3.zero;

            int index = tilemapController.gridCells.IndexOf(cell);
            stationGroups[index].station = instance;

            Debug.Log($"[Restore] 복원 완료: '{info.id}' → '{info.gridCellName}'");
        }
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

            GameObject station = group.station;
            if (station == null) continue;

            string stationType = station.name; // 또는 station.GetComponent<Station>().stationData.id

            TotalStationCount++;               // 스테이션 개수 카운트

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

        // 복원 조건
        if (newPhase == GamePhase.EditStation || newPhase == GamePhase.Day || newPhase == GamePhase.Opening)
        {
            Debug.Log($"[StationManager] 복원 조건 진입: newPhase = {newPhase}");

            Debug.Log("[StationManager] 복원용 stationGroups 초기화 완료");
            SaveLoadManager.RestoreStationState();  // 세이브로드매니저에서 제이슨으로 스테이션 상태를 복원
        }

        // 저장 & 제거 조건
        if (previousPhase == GamePhase.EditStation || previousPhase == GamePhase.Day)
        {
            Debug.Log($"[StationManager] 저장 조건 진입: previousPhase = {previousPhase}");

            SetStations(); // 현재 상태 갱신

            var stationInfos = GenerateStationSaveData();
            Debug.Log($"[StationManager] 저장 대상 Station 수: {stationInfos.Count}");

            SaveLoadManager.SaveStationData(stationInfos); // JSON 저장

            Debug.Log("[StationManager] station.json 저장 완료");
        }

        // 제거만 조건
        if (previousPhase == GamePhase.Closing)
        {
            stationGroups.Clear(); // 기존 스테이션 그룹 초기화
        }

        previousPhase = newPhase;
    }
}
