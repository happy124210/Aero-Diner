using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpeakerData", menuName = "Data/Speaker Data")]
public class SpeakerData : ScriptableObject
{
    public string id;          // 예: "KAYA"
    public string speakerName; // 예: "카야"
    
    public Dictionary<Expression, Sprite> portraits = new();

    public Sprite GetPortraitByExpression(Expression expression)
    {
        return portraits.TryGetValue(expression, out Sprite portrait) 
            ? portrait 
            : portraits.GetValueOrDefault(Expression.Default);
    }
}

public enum Expression
{
    Default,
    Happy,
    Sad,
    Surprised,
    Angry
}
