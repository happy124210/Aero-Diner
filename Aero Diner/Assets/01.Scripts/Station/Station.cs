using System;
using UnityEngine;

[Serializable]
public class Station
{
    [Header("기본 데이터")]
    public StationData stationData;

    [Header("플레이어 진행 상황")]
    public bool isUnlocked; // 스테이션이 해금되었는지
    public bool isSelected; // 스테이션이 영업에 사용되는지

    public Station(StationData data, bool unlocked = false, bool selected = false)
    {
        stationData = data;
        isUnlocked = unlocked;
        isSelected = selected;
    }

    public string StationName => stationData?.displayName ?? "Unknown";
    public int Price => stationData?.stationCost ?? 0;
    public bool CanServeToday => isUnlocked && isSelected;
}
