using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using static SaveData;

public class StationManager : Singleton<StationManager>
{
    [Header("참조")]
    [SerializeField] private TilemapController tilemapController;
    
    [Header("설비 세트")]
    [SerializeField] private List<GameObject> stationPrefabs = new();
    
    [Header("디버깅")]
    [SerializeField] private bool showDebugInfo;
    [SerializeField] private List<string> startStationIds = new() {"s1", "s2", "s3", "s5", "s8", "s15", "s16", "s17", "s24"};
    
    // 데이터베이스들
    private Dictionary<string, Station> stationDatabase = new(); // 전체 Station타입 스테이션 데이터 담음
    private Dictionary<string, GameObject> stationPrefabDatabase = new(); 
    private HashSet<string> unlockedStationIds = new();
    
    // 현재 상태 관리
    [SerializeField] private List<StationGroup> stationGroups = new();         // 각 GridCell 아래에 있는 Station 리스트
    private Dictionary<string, (int gridCellCount, int storageGridCellCount)> stationTypeCounts = new();
    
    #region public getters & methods
    
    public Dictionary<string, Station> StationDatabase => stationDatabase;
    
    // id로 Station / StationData 조회
    public Station FindStationById(string id) => stationDatabase.GetValueOrDefault(id);
    public StationData FindStationDataById(string id) => stationDatabase.GetValueOrDefault(id).stationData;
    
    public bool IsUnlocked(string id) => unlockedStationIds.Contains(id);
    
    // Station 개수 조회
    public int GetStationPlacedCount(string id) => stationTypeCounts.TryGetValue(id, out var counts) ? counts.gridCellCount : 0;
    public int GetStationStoredCount(string id) => stationTypeCounts.TryGetValue(id, out var counts) ? counts.storageGridCellCount : 0;
    
    // public methods
    public List<Station> GetAllStations() => stationDatabase.Values.ToList();
    public List<Station> GetUnlockedStations() => stationDatabase.Values.Where(station => station.isUnlocked).ToList();
    public HashSet<string> GetUnlockedStationIds() => stationDatabase.Keys.ToHashSet();
    
    // 설비 개수
    private int totalStationCount; // 전체 스테이션 개수
    private int gridCellStationCount; // 영업 공간에 배치되어있는 개수
    private int storageGridCellStationCount; // 보관 공간에 배치되어있는 개수
   
    #endregion

    #region class
    
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
    
    #endregion

    protected override void Awake()
    {
        if (showDebugInfo) Debug.Log("[StationManager] Awake에서 Instance 초기화됨");
        base.Awake();

        InitializeStationDatabase();
        InitializePrefabDatabase();
        LoadStationDatabase();
    }

    private void Start()
    {
        InitializeStations();
    }

    public void InitializeStations()
    {
        var phase = GameManager.Instance.CurrentPhase;
        bool didLoad = false;

        if (phase == GamePhase.Day || phase == GamePhase.Opening || phase == GamePhase.Operation)
        {
            StationLoad(phase);
            didLoad = true;
        }

        tilemapController.FindGridCells();
        if (showDebugInfo) Debug.Log($"[StationManager] GridCell 수: {tilemapController.gridCells.Count}");

        if (!didLoad)
        {
            SetStations();
        }
        
        CountStationsPerCellType();
    }

    public void SetTilemapController(TilemapController controller)
    {
        tilemapController = controller;
        if (showDebugInfo) Debug.Log("[StationManager] 타일맵 컨트롤러 연결 완료");
    }

    #region Station 데이터베이스 & 해금 관리

    private void InitializeStationDatabase()
    {
        stationDatabase.Clear();
        StationData[] allStations = Resources.LoadAll<StationData>(StringPath.STATION_DATA_PATH);
        foreach (var data in allStations)
        {
            if (!string.IsNullOrEmpty(data.id))
            {
                stationDatabase[data.id] = new Station(data);
            }
        }
    }

    public void SaveStationDatabase()
    {
        var data = SaveLoadManager.LoadGame() ?? new SaveData();
        data.stationDatabase = new HashSet<string>(GetUnlockedStationIds());
        SaveLoadManager.SaveGame(data);
        
        if (showDebugInfo) Debug.Log($"[StationManager] 해금 설비 저장됨: {data.stationDatabase.Count}개");
    }
    
    /// <summary>
    /// 스테이션 데이터베이스 로딩
    /// </summary>
    private void LoadStationDatabase()
    {
        SaveData saveData = SaveLoadManager.LoadGame();
        if (saveData == null) return;

        if (unlockedStationIds == null || unlockedStationIds.Count == 0)
        {
            // 해금 데이터 없을 때 시작설비 해금
            if (startStationIds == null) Debug.LogError("startMenuId가 없음");
            foreach (var id in startStationIds)
            {
                UnlockStation(id);
            }

            return;
        }
        
        // 해금 데이터 Id 받아서 복원
        foreach (var unlockedId in saveData.stationDatabase)
        {
            UnlockStation(unlockedId);
        }

        if (showDebugInfo) Debug.Log($"StationManager]: 전체 {stationDatabase.Count}개  데이터베이스 생성 완료");
    }
    
    /// <summary>
    /// 프리팹 리스트를 기반으로 ID-프리팹 딕셔너리 생성
    /// </summary>
    private void InitializePrefabDatabase()
    {
        stationPrefabDatabase.Clear();
        foreach (var prefab in stationPrefabs)
        {
            var stationComponent = prefab.GetComponent<IMovableStation>();
            if (stationComponent != null && stationComponent.StationData != null && !string.IsNullOrEmpty(stationComponent.StationData.id))
            {
                stationPrefabDatabase[stationComponent.StationData.id] = prefab;
            }
            else
            {
                Debug.LogWarning($"[StationManager] '{prefab.name}' 프리팹에 유효한 StationData ID가 없어 데이터베이스에 추가할 수 없습니다.");
            }
        }
    }

    public void UnlockStation(string stationId)
    {
        if (string.IsNullOrEmpty(stationId)) return;
        
        Station stationToUnlock = FindStationById(stationId);
        if (stationToUnlock == null) return;
        
        stationToUnlock.isUnlocked = true;
    }
    
    // cheater
    public void UnlockAllStations()
    {
        foreach (string stationId in stationDatabase.Keys)
        {
            UnlockStation(stationId);
        }
    }

    #endregion

    #region 저장 & 복원

    /// <summary>
    /// 현재 배치된 스테이션 정보를 StationData.id를 기준으로 생성합니다.
    /// </summary>
    public List<StationSaveInfo> GenerateStationSaveData()
    {
        var saveList = new List<StationSaveInfo>();
        for (int i = 0; i < stationGroups.Count; i++)
        {
            var group = stationGroups[i];
            if (group.station == null) continue;

            var stationComponent = group.station.GetComponent<IMovableStation>();
            if (stationComponent == null || stationComponent.StationData == null) continue;
            
            var info = new StationSaveInfo
            {
                id = stationComponent.StationData.id, // 데이터의 ID를 직접 사용
                gridCellName = tilemapController.gridCells[i].name
            };
            saveList.Add(info);
        }
        return saveList;
    }

    /// <summary>
    /// 프리팹 데이터베이스를 사용하여 스테이션 복원
    /// </summary>
    public void RestoreStations(List<StationSaveInfo> infos, GamePhase currentPhase)
    {
        stationGroups.Clear();
        for (int i = 0; i < tilemapController.gridCells.Count; i++)
            stationGroups.Add(new StationGroup());

        foreach (var info in infos)
        {
            StationData data = FindStationDataById(info.id);
            if (data == null || !stationPrefabDatabase.TryGetValue(info.id, out GameObject prefab))
            {
                if (showDebugInfo) Debug.LogError($"[Restore] StationData 또는 Prefab을 찾을 수 없음: {info.id}");
                continue;
            }

            GameObject cell = tilemapController.gridCells.FirstOrDefault(c => c.name == info.gridCellName);
            if (cell == null)
            {
                if (showDebugInfo) Debug.LogError($"[Restore] 셀을 찾을 수 없음: {info.gridCellName}");
                continue;
            }

            GameObject instance = Instantiate(prefab, cell.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.GetComponent<BaseStation>()?.Initialize(data);
            
            bool isStorageCell = cell.GetComponent<StorageGridCell>() != null;
            bool shouldActivate = currentPhase == GamePhase.EditStation || currentPhase == GamePhase.Day;
            instance.SetActive(isStorageCell ? shouldActivate : true);
            
            int index = tilemapController.gridCells.IndexOf(cell);
            stationGroups[index].station = instance;
        }
    }
    
    /// <summary>
    /// 현재 상태의 스테이션 정보를 JSON으로 저장
    /// </summary>
    public void StationSave()
    {
        SetStations(); // 현재 상태 갱신

        var stationInfos = GenerateStationSaveData();
        Debug.Log($"[StationManager] 저장 대상 Station 수: {stationInfos.Count}");

        SaveLoadManager.SaveStationData(stationInfos); // JSON 저장

        Debug.Log($"[StationManager] 저장 완료");
    }

    /// <summary>
    /// 현재 게임 상태에 맞는 스테이션을 불러옴
    /// </summary>
    /// <param name="phaseObj"></param>
    public void StationLoad(object phaseObj)
    {
        GamePhase newPhase = (GamePhase)phaseObj;
        DestroyCurrentStations();
        tilemapController.FindGridCells();
        SaveLoadManager.RestoreStationState(newPhase);
        if (showDebugInfo) Debug.Log("[StationManager] 불러오기 완료");
    }
    
    #endregion

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
    /// 각 GridCell 타입별로 스테이션 개수를 세고 출력
    /// </summary>
    private void CountStationsPerCellType()
    {
        if (tilemapController == null || stationGroups == null || tilemapController.gridCells.Count != stationGroups.Count)
        {
            if (showDebugInfo) Debug.LogError("[StationManager] 데이터 정합성 오류: gridCell 개수와 stationGroup 개수가 맞지 않습니다.");
            return;
        }
        
        stationTypeCounts.Clear(); 
        totalStationCount = 0;
        gridCellStationCount = 0;
        storageGridCellStationCount = 0;

        for (int i = 0; i < stationGroups.Count; i++)
        {
            GameObject gridCell = tilemapController.gridCells[i];
            StationGroup group = stationGroups[i];

            GameObject stationGO = group.station;
            if (stationGO == null) continue;
            
            var stationComponent = stationGO.GetComponent<IMovableStation>();
            if (stationComponent == null || stationComponent.StationData == null)
            {
                if(showDebugInfo) Debug.LogWarning($"[StationManager] '{stationGO.name}'에 IMovableStation 컴포넌트나 StationData가 없습니다.");
                continue;
            }
            string stationType = stationComponent.StationData.id;
            
            
            string cellType = gridCell.name.StartsWith("StorageGridCell") ? "Storage" : "Grid";
            
            totalStationCount++;

            if (cellType == "Grid") gridCellStationCount++;
            else if (cellType == "Storage") storageGridCellStationCount++;

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
        
        foreach (var kvp in stationTypeCounts)
        {
            string stationName = kvp.Key;
            var (gridCount, storageCount) = kvp.Value;
            if (showDebugInfo) Debug.Log($"[StationManager] {stationName} → GridCell: {gridCount}, StorageGridCell: {storageCount}");
        }

        if (showDebugInfo) Debug.Log($"[StationManager] 전체 스테이션 수: {totalStationCount} (GridCell: {gridCellStationCount}, StorageGridCell: {storageGridCellStationCount})");
    }

    /// <summary>
    /// 보관장소에 스테이션 생성
    /// 상점에서 구매했을 때 호출됨
    /// </summary>
    /// <param name="id">StationData의 고유 ID</param>
    public bool CreateStationInStorage(string id)
    {
        // StationData 확인
        if (!stationDatabase.ContainsKey(id))
        {
            Debug.LogError($"[StationManager] StationData를 찾을 수 없음: {id}");
            return false;
        }
        
        if (!stationPrefabDatabase.TryGetValue(id, out GameObject prefab))
        {
            Debug.LogError($"[StationManager] ID에 해당하는 프리팹을 찾을 수 없음: {id}");
            return false;
        }

        // Storage 셀 중 비어있는 셀 찾기
        GameObject targetCell = null;
        foreach (var cell in tilemapController.gridCells)
        {
            // StorageGridCell 컴포넌트가 있는 셀만 대상
            if (cell.GetComponent<StorageGridCell>() == null)
                continue;

            // 자식 중에 이미 다른 스테이션이 있는지 확인
            bool hasStation = cell.GetComponentsInChildren<IMovableStation>(true).Length > 0;
            if (!hasStation)
            {
                targetCell = cell;
                break;
            }
        }

        if (targetCell == null)
        {
            Debug.LogWarning($"[StationManager] 빈 Storage 셀을 찾을 수 없음 - 스테이션 생성 실패: {id}");
            return false;
        }

        // 스테이션 인스턴스 생성
        GameObject instance = Instantiate(prefab, targetCell.transform);
        instance.transform.localPosition = Vector3.zero;
        instance.SetActive(true); 

        // stationGroups 동기화
        int index = tilemapController.gridCells.IndexOf(targetCell);
        if (index >= 0 && index < stationGroups.Count)
        {
            stationGroups[index].station = instance;
        }

        if (showDebugInfo) Debug.Log($"[StationManager] 스토리지에 Station 생성됨: {id} → {targetCell.name}");
        return true;
    }


    /// <summary>
    /// 모든 GridCell 아래의 IMovableStation 오브젝트를 제거
    /// </summary>
    private void DestroyCurrentStations()
    {
        if (tilemapController == null)
        {
            if (showDebugInfo) Debug.LogError("[StationManager] tilemapController가 없습니다.");
            return;
        }

        foreach (GameObject gridCell in tilemapController.gridCells)
        {
            var stations = gridCell.GetComponentsInChildren<IMovableStation>(includeInactive: true);

            foreach (var station in stations)
            {
                var transform = station.GetTransform();
                if (transform != null)
                {
                    if (showDebugInfo) Debug.Log($"[StationManager] 제거됨: {transform.gameObject.name}");
                    Destroy(transform.gameObject);
                }
            }
        }

        if (showDebugInfo) Debug.Log("[StationManager] 기존 스테이션 제거 완료");
    }

    #region 튜토리얼 관리

    public void ActivateStation(string id) => SetInteractableState(id, true);
    public void DeactivateStation(string id) => SetInteractableState(id, false);
    
    /// <summary>
    /// 주어진 Station ID를 기준으로 stationGroups 리스트에서 해당 Station의 InteractableTutorial 컴포넌트를 찾아 비활성화
    /// </summary>
    /// <param name="id"> Station의 고유 식별자 </param>
    /// <param name="isInteractable"> 활성화 여부 </param>
    private void SetInteractableState(string id, bool isInteractable)
    {
        SetStations();
        
        foreach (var group in stationGroups)
        {
            var stationGO = group.station;
            
            if (!stationGO)
            {
                continue;
            }

            var data = stationGO.GetComponent<IMovableStation>().StationData;
            if (data && data.id == id)
            {
                var interactableComponent = stationGO.GetComponent<InteractableTutorial>();
                if (interactableComponent)
                {
                    interactableComponent.SetInteractable(isInteractable);
                    if (showDebugInfo) Debug.Log($"[StationManager] Station '{id}'의 InteractableTutorial 상태를 {isInteractable}(으)로 변경");
                    return;
                }

                Debug.LogWarning($"[StationManager] Station '{id}'에서 InteractableTutorial 컴포넌트를 찾지 못함");
                return;
            }
        }

        Debug.LogWarning($"[StationManager] StationGroups에서 ID '{id}'를 가진 Station을 찾을 수 없음");
    }

    /// <summary>
    /// 특정 설비에 특정 음식들이 모두 배치되어 있는지 여부 판단
    /// </summary>
    public bool CheckIngredients(string stationId, string[] requiredIngredients)
    {
        if (requiredIngredients == null || requiredIngredients.Length == 0) return false;
    
        SetStations();

        var stationGroup = stationGroups.FirstOrDefault(g => g.station && g.station.GetComponent<IMovableStation>()?.StationData.id == stationId);

        if (stationGroup == null || !stationGroup.station)
        {
            Debug.LogWarning($"[StationManager] StationGroups에서 ID '{stationId}'를 가진 Station을 찾을 수 없음");
            return false;
        }

        var baseStation = stationGroup.station.GetComponent<BaseStation>();
        if (!baseStation)
        {
            if (showDebugInfo) Debug.LogWarning($"[StationManager] Station '{stationId}'에서 BaseStation 컴포넌트를 찾지 못함");
            return false;
        }

        var currentIngredients = baseStation.currentIngredients;
        
        foreach (var required in requiredIngredients)
        {
            if (!currentIngredients.Contains(required))
            {
                if (showDebugInfo) Debug.Log($"[StationManager] Station '{stationId}'에 '{required}'이(가) 없음. 조건 미충족.");
                return false;
            }
        }
        
        if (showDebugInfo) Debug.Log($"[StationManager] Station '{stationId}'에 모든 필요 재료가 배치됨. 조건 충족!");
        return true;
    }
    
    /// <summary>
    /// 특정 설비 아래 FoodData의 오브젝트가 있는지 확인
    /// (최종 결과물 확인할 때 사용)
    /// </summary>
    public bool CheckObjectOnStation(string stationId, string objectId)
    {
        if (string.IsNullOrEmpty(objectId)) return false;

        SetStations();

        var stationGroup = stationGroups.FirstOrDefault(g => g.station && g.station.GetComponent<IMovableStation>()?.StationData.id == stationId);
        if (stationGroup == null || !stationGroup.station) return false;

        var foodDisplays = stationGroup.station.GetComponentsInChildren<FoodDisplay>();
        return foodDisplays.Any(display => display.foodData && display.foodData.id == objectId);
    }
    
  #if UNITY_EDITOR
    /// <summary>
    /// 디버깅용
    /// 특정 Station ID의 currentIngredients 목록을 콘솔에 출력하는 테스트 메서드
    /// </summary>
    public List<string> GetCurrentIngredients(string stationId)
    {
        SetStations();

        foreach (var group in stationGroups)
        {
            var stationGO = group.station;
            if (!stationGO) continue;

            var data = stationGO.GetComponent<IMovableStation>()?.StationData;
            if (data != null && data.id == stationId)
            {
                var baseStation = stationGO.GetComponent<BaseStation>();
                if (!baseStation) return new List<string>();

                return new List<string>(baseStation.currentIngredients);
            }
        }

        return new List<string>();
    }
    #endif
    
    /// <summary>
    /// 특정 그리드 셀에 특정 ID의 설비가 배치되었는지 확인
    /// </summary>
    /// <param name="stationId">확인할 설비의 ID</param>
    /// <param name="gridCellName">확인할 그리드 셀의 이름</param>
    public bool CheckStationPlacedOnCell(string stationId, string gridCellName)
    {
        if (string.IsNullOrEmpty(stationId) || string.IsNullOrEmpty(gridCellName)) return false;
        SetStations();
        
        for (int i = 0; i < stationGroups.Count; i++)
        {
            // 현재 인덱스의 그리드 셀 이름 다르면 건너뜀
            if (tilemapController.gridCells[i].name != gridCellName)
            {
                continue;
            }

            // 해당 셀에 스테이션이 있는지, 일치하는지 확인
            var station = stationGroups[i].station;
            if (station)
            {
                var stationData = station.GetComponent<IMovableStation>()?.StationData;
                if (stationData && stationData.id == stationId)
                {
                    if (showDebugInfo) Debug.Log($"[StationManager] '{gridCellName}'에 '{stationId}'가 배치됨");
                    return true;
                }
            }
        }

        return false;
    }
    
    #endregion
}
