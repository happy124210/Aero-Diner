using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    // 🔹 날짜 및 진행 정보
    public int currentDay;
    public float totalEarnings;

    // 🔹 메뉴 관련 정보
    public HashSet<string> unlockedMenuIds;         // 해금된 메뉴 ID들 (중복 방지용)

    // 🔹 플레이어 상태
    public string heldItemId;
    public Vector2 playerPosition;

    // 🔹 설정 정보 (기존 PlayerPrefs → 통합 저장)
    public float bgmVolume;
    public float sfxVolume;

    // 🔹 키 바인딩 (Newtonsoft.Json은 Dictionary도 가능)
    public Dictionary<string, string> keyBindings;   // ex: "MoveUp" → "W"

    // 🔹 튜토리얼 상태
    public bool isTutorialCompleted;
}
