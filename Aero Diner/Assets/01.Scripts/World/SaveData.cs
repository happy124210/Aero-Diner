using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    //  날짜 및 진행 정보
    public int currentDay;
    public int totalEarnings;

    //  메뉴 관련 정보
    public HashSet<string> menuDatabase;         // 해금된 메뉴 ID들 (중복 방지용)
    public HashSet<string> stationDatabase;         // 해금된 스테이션 ID들 (중복 방지용)

    //  설정 정보 (기존 PlayerPrefs → 통합 저장)
    public float bgmVolume = 0.5f;
    public float sfxVolume = 0.5f;

    //  키 바인딩 (Newtonsoft.Json은 Dictionary도 가능)
    public Dictionary<string, string> keyBindings;   // ex: "MoveUp" → "W"

    // 화면 옵션
    public int screenModeIndex = 2;        // 기본값: 전체화면
    public int resolutionIndex = 0;        // 기본값: 목록 첫 번째

    public SaveData()
    {
        menuDatabase = new HashSet<string>();
        stationDatabase = new HashSet<string>();
        keyBindings = new Dictionary<string, string>();
    }

    public class StationSaveInfo
    {
        public string id;
        public string gridCellName;
    }

    public List<string> unlockedStationIds = new();

}
