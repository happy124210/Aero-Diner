using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MenuSaveHandler
{
    /// 저장
    public static void SaveMenuDatabase()
    {
        var data = SaveLoadManager.LoadGame() ?? new SaveData();
        data.menuDatabase = new HashSet<string>(MenuManager.Instance.GetPlayerMenuIds());
        SaveLoadManager.SaveGame(data);

        Debug.Log($"[MenuSaveHandler] 해금 메뉴 저장됨: {data.menuDatabase.Count}개");
    }

    /// 불러오기 (초기화 시 호출)
    public static void LoadMenuDatabase()
    {
        var data = SaveLoadManager.LoadGame();
    }
}
