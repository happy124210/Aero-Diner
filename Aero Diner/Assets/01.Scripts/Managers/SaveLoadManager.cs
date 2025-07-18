using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private static string savePath => Path.Combine(Application.persistentDataPath, "save.json");

    // 저장
    public static void SaveGame(SaveData data)
    {
        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(savePath, json);

            if (Instance?.showDebugInfo == true)
                Debug.Log($"[SaveLoadManager] 게임 저장 완료: {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoadManager] 저장 실패: {e.Message}");
        }
    }

    // 불러오기
    public static SaveData LoadGame()
    {
        if (!File.Exists(savePath))
        {
            if (Instance?.showDebugInfo == true)
                Debug.LogWarning("[SaveLoadManager] 저장 파일이 없습니다.");

            return null;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonConvert.DeserializeObject<SaveData>(json);

            if (Instance?.showDebugInfo == true)
                Debug.Log("[SaveLoadManager] 게임 불러오기 완료");

            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoadManager] 불러오기 실패: {e.Message}");
            return null;
        }
    }

    // 저장 파일 존재 여부
    public static bool HasSaveData()
    {
        bool exists = File.Exists(savePath);

        if (Instance?.showDebugInfo == true)
            Debug.Log($"[SaveLoadManager] 저장 파일 존재 여부: {exists}");

        return exists;
    }

    // 저장 파일 삭제 (New Game 시)
    public static void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);

            if (Instance?.showDebugInfo == true)
                Debug.Log("[SaveLoadManager] 저장 파일 삭제됨");
        }
        else
        {
            if (Instance?.showDebugInfo == true)
                Debug.LogWarning("[SaveLoadManager] 삭제할 저장 파일이 없습니다.");
        }
    }
    public static void ResetProgressOnly()
    {
        var data = LoadGame();
        if (data == null)
        {
            data = new SaveData();
            Debug.LogWarning("[SaveLoadManager] 기존 세이브 없음 → 새 SaveData로 초기화함");
        }

        // 현재 바인딩 정보 저장
        var preservedKeyBindings = data.keyBindings != null
            ? new Dictionary<string, string>(data.keyBindings)
            : new Dictionary<string, string>();

        // 진행 정보 초기화
        data.currentDay = 1;
        data.totalEarnings = 0;
        data.menuDatabase.Clear();

        // 다시 보존한 키 바인딩 할당
        data.keyBindings = preservedKeyBindings;

        SaveGame(data);

        if (Instance?.showDebugInfo == true)
            Debug.Log("[SaveLoadManager] 진행 정보만 초기화됨 (옵션 및 키 바인딩 유지)");
    }
}
