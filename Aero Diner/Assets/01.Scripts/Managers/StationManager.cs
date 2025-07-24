using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StationManager : Singleton<StationManager>
{
    private List<StationData> stationDataList;
    private List<Station> stationDatabase = new();      // 전체 Station타입 스테이션 데이터 담음

    [SerializeField] private List<GameObject> stationPrefabs = new();

    [Header("디버깅")]
    [SerializeField] private bool showDebugInfo;
    
    // Station 조회는 전부 id로 함
    // 스테이션 조회하기
    public Station FindStationById(string id) => stationDatabase.FirstOrDefault(m => m.stationData.id == id);

    // 플레이어 해제 스테이션만 가져오기
    public List<Station> GetUnlockedStations() => stationDatabase.Where(menu => menu.isUnlocked).ToList();

    // 스테이션 조회
    public List<Station> GetAllStations() => stationDatabase;
    public string[] GetPlayerStationIds() => stationDatabase.Where(m => m.isUnlocked).Select(m => m.stationData.id).ToArray(); // 해금한 스테이션만

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

        InitializeStationDatabase();
    }

    /// <summary>
    /// 스테이션 데이터베이스 초기화
    /// </summary>
    private void InitializeStationDatabase()
    {
        stationDatabase.Clear();

        foreach (var data in stationDataList)
        {
            stationDatabase.Add(new Station(data));
        }

        if (showDebugInfo) Debug.Log($"StationManager]: 전체 {stationDatabase.Count}개  데이터베이스 생성 완료");
    }

    /// <summary>
    /// 보관장소에 스테이션 생성
    /// </summary>

}
