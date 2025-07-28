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
        Instance = this;
        if (showDebugInfo) Debug.Log("[StationManager] Awake에서 Instance 초기화됨");
        base.Awake();

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
        if (showDebugInfo) Debug.Log("[StationManager] 타일맵 컨트롤러 연결 완료");
    }


    /// <summary>
    /// GridCell 아래의 오브젝트들을 수집해서 구조화
    /// </summary>
    private void SetStations()
    {
        if (tilemapController == null)
        {
            if (showDebugInfo) Debug.LogError("[StationManager] tilemapController가 할당되지 않았습니다.");
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
            if (showDebugInfo) Debug.Log($"[StationManager] '{cellName}' 셀에 스테이션: {stationName}");
        }

        if (showDebugInfo) Debug.Log($"[StationManager] 총 {stationGroups.Count}개의 GridCell에서 스테이션을 수집했습니다.");
    }

    /// <summary>
    /// 현재 배치된 스테이션 정보를 기반으로 StationSaveInfo 리스트를 생성
    /// - 스테이션 이름에서 "(Clone)" 문자열을 제거한 ID를 저장
    /// </summary>
    public List<StationSaveInfo> GenerateStationSaveData()
    {
        var saveList = new List<StationSaveInfo>();

        for (int i = 0; i < stationGroups.Count; i++)
        {
            var group = stationGroups[i];
            var cell = tilemapController.gridCells[i];

            if (group.station == null)
            {
                if (showDebugInfo) Debug.Log($"[Generate] stationGroups[{i}]의 station이 null → 저장 제외됨 (cell: '{cell.name}')");
                continue;
            }

            // 프리팹 이름에서 (Clone) 제거
            string cleanId = group.station.name.Replace("(Clone)", "").Trim();

            var info = new StationSaveInfo
            {
                id = cleanId,
                gridCellName = cell.name
            };

            saveList.Add(info);

            if (showDebugInfo) Debug.Log($"[Generate] 저장됨: id = '{info.id}', cell = '{info.gridCellName}'");
        }

        return saveList;
    }

    /// <summary>
    /// 저장된 Station 정보를 바탕으로 스테이션들을 복원
    /// 각 GridCell에 맞는 Station 프리팹을 Instantiate하여 배치
    /// StorageGridCell이 붙은 셀에 배치되는 Station은 초기에는 비활성화 상태로 시작
    /// </summary>
    /// <param name="infos">StationSaveInfo 리스트 (station.json에서 불러온 데이터)</param>
    public void RestoreStations(List<StationSaveInfo> infos)
    {
        // stationGroups 초기화 (GridCell 수만큼)
        stationGroups.Clear();
        for (int i = 0; i < tilemapController.gridCells.Count; i++)
            stationGroups.Add(new StationGroup());

        foreach (var info in infos)
        {
            // 프리팹 로드
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/Stations/{info.id}");
            if (prefab == null)
            {
                if (showDebugInfo) Debug.LogError($"[Restore] 프리팹 못 찾음: {info.id}");
                continue;
            }

            // 셀 찾기
            GameObject cell = tilemapController.gridCells.FirstOrDefault(c => c.name == info.gridCellName);
            if (cell == null)
            {
                if (showDebugInfo) Debug.LogError($"[Restore] 셀 못 찾음: {info.gridCellName}");
                continue;
            }

            // 프리팹 Instantiate 및 위치 지정
            GameObject instance = Instantiate(prefab, cell.transform);
            instance.transform.localPosition = Vector3.zero;

            // Storage 셀이라면 Station을 비활성화된 상태로 시작
            bool isStorageCell = cell.GetComponent<StorageGridCell>() != null;
            if (isStorageCell)
            {
                instance.SetActive(false);
                if (showDebugInfo) Debug.Log($"[Restore] StorageCell에서 비활성화 상태로 복원됨: {info.id}");
            }

            // StationGroup에 등록
            int index = tilemapController.gridCells.IndexOf(cell);
            stationGroups[index].station = instance;

            if (showDebugInfo) Debug.Log($"[Restore] 복원 완료: '{info.id}' → '{info.gridCellName}'");
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
            if (showDebugInfo) Debug.Log($"[StationManager] {stationName} → GridCell: {gridCount}, StorageGridCell: {storageCount}");
        }

        if (showDebugInfo) Debug.Log($"[StationManager] 전체 스테이션 수: {TotalStationCount} (GridCell: {GridCellStationCount}, StorageGridCell: {StorageGridCellStationCount})");
    }
    /// <summary>
    /// 특정 ID의 Station이 현재 GridCell에 배치된 개수
    /// </summary>
    public int GetPlacedStationCount(string id)
    {
        int count = 0;

        for (int i = 0; i < stationGroups.Count; i++)
        {
            var group = stationGroups[i];
            var station = group.station;
            if (station == null) continue;

            // "(Clone)" 제거한 이름이 ID와 일치할 경우
            string stationId = station.name.Replace("(Clone)", "").Trim();

            // GridCell (보관소 제외)
            var cell = tilemapController.gridCells[i];
            bool isStorage = cell.GetComponent<StorageGridCell>() != null;

            if (!isStorage && stationId == id)
                count++;
        }

        return count;
    }

    /// <summary>
    /// 특정 ID의 Station이 현재 StorageGridCell에 보관 중인 개수
    /// </summary>
    public int GetStoredStationCount(string id)
    {
        int count = 0;

        for (int i = 0; i < stationGroups.Count; i++)
        {
            var group = stationGroups[i];
            var station = group.station;
            if (station == null) continue;

            string stationId = station.name.Replace("(Clone)", "").Trim();

            var cell = tilemapController.gridCells[i];
            bool isStorage = cell.GetComponent<StorageGridCell>() != null;

            if (isStorage && stationId == id)
                count++;
        }

        return count;
    }
    // 해금된 스테이션 ID 목록
    private HashSet<string> unlockedStationIds = new HashSet<string>();

    public bool IsUnlocked(string id) => unlockedStationIds.Contains(id);

    public void UnlockStation(string id)
    {
        if (!string.IsNullOrEmpty(id))
            unlockedStationIds.Add(id);
    }

    public void LockStation(string id)
    {
        if (unlockedStationIds.Contains(id))
            unlockedStationIds.Remove(id);
    }

    public List<string> GetUnlockedStations()
    {
        return unlockedStationIds.ToList();
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

    private void OnGamePhaseChanged(object phaseObj)
    {
        GamePhase newPhase = (GamePhase)phaseObj;

        // 복원 조건
        if (newPhase == GamePhase.EditStation || newPhase == GamePhase.Day || newPhase == GamePhase.Opening)
        {
            if (showDebugInfo) Debug.Log($"[StationManager] 복원 조건 진입: newPhase = {newPhase}");

            tilemapController.FindGridCells(); // 강제 초기화
            if (showDebugInfo) Debug.Log("[StationManager] 복원용 stationGroups 초기화 완료");
            SaveLoadManager.RestoreStationState();  // 세이브로드매니저에서 제이슨으로 스테이션 상태를 복원
        }

        // 저장 & 제거 조건
        if (newPhase == GamePhase.EditStation || newPhase == GamePhase.Day || newPhase == GamePhase.Opening)
        {
            SetStations(); // 현재 상태 갱신

            var stationInfos = GenerateStationSaveData();
            if (showDebugInfo) Debug.Log($"[StationManager] 저장 대상 Station 수: {stationInfos.Count}");

            SaveLoadManager.SaveStationData(stationInfos); // JSON 저장

            if (showDebugInfo) Debug.Log("[StationManager] station.json 저장 완료");
        }

        // 제거만 조건
        if (newPhase == GamePhase.Closing)
        {
            stationGroups.Clear(); // 기존 스테이션 그룹 초기화
        }
    }
}
