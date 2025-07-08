using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MenuSaveHandler
{
    /// 저장
    public static void SaveUnlockedMenus()
    {
        var data = SaveLoadManager.LoadGame() ?? new SaveData();
        data.unlockedMenuIds = new HashSet<string>(MenuManager.Instance.GetPlayerMenuIds());
        SaveLoadManager.SaveGame(data);

        Debug.Log($"[MenuSaveHandler] 해금 메뉴 저장됨: {data.unlockedMenuIds.Count}개");
    }

    /// 불러오기 (초기화 시 호출)
    public static void LoadUnlockedMenus()
    {
        var data = SaveLoadManager.LoadGame();
        if (data?.unlockedMenuIds == null) return;

        foreach (string id in data.unlockedMenuIds)
        {
            MenuManager.Instance.UnlockMenu(id); // 내부 중복 방지
        }
    }
}
