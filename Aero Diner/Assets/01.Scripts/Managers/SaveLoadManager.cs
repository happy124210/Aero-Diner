using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class SaveLoadManager : Singleton<SaveLoadManager>
{

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
            Debug.Log($"[SaveLoadManager] 게임 저장 완료: {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoadManager] 저장 실패: {e.Message}");
        }
    }

    //  불러오기
    public static SaveData LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("[SaveLoadManager] 저장 파일이 없습니다.");
            return null;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonConvert.DeserializeObject<SaveData>(json);
            Debug.Log("[SaveLoadManager] 게임 불러오기 완료");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoadManager] 불러오기 실패: {e.Message}");
            return null;
        }
    }

    //  저장 파일 존재 여부
    public static bool HasSaveData()
    {
        return File.Exists(savePath);
    }

    //  저장 파일 삭제 (New Game 시)
    public static void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("[SaveLoadManager] 저장 파일 삭제됨");
        }
    }
}
