using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 화자 정보
/// </summary>
[System.Serializable]
public class SpeakerData : ScriptableObject
{
    public string id;          // SpeakerData의 id
    public string speakerName; // UI용 화자 이름 
    public Dictionary<Expression, Sprite> portraits;  // 각 표정 스프라이트 딕셔너리

    public Sprite GetPortraitByExpression(Expression expression) => portraits[expression];
}

public enum Expression
{
    Default,
    Happy,
    Sad,
    Surprised,
    Angry
}