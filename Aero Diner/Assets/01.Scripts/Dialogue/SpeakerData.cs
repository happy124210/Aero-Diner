using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ를 사용하면 더 간단해집니다.

// 이 enum은 그대로 사용합니다.
public enum Expression
{
    Default,
    Happy,
    Sad,
    Angry
}

// 이 클래스를 SpeakerData 파일 안에 넣거나 별도 파일로 만들어도 됩니다.
// [System.Serializable]을 붙여야 유니티가 데이터를 저장할 수 있습니다.
[System.Serializable]
public class PortraitEntry
{
    public Expression expression;
    public Sprite portrait;
}

[CreateAssetMenu(fileName = "SpeakerData", menuName = "Data/Speaker Data")]
public class SpeakerData : ScriptableObject
{
    public string id;
    public string speakerName;
    

    public List<PortraitEntry> portraits = new();
    private Dictionary<Expression, Sprite> portraitDict;
    private bool isDictInitialized;

    // 딕셔너리를 초기화하는 메소드
    private void InitializeDictionary()
    {
        if (isDictInitialized) return;

        portraitDict = new Dictionary<Expression, Sprite>();
        foreach (var entry in portraits)
        {
            portraitDict[entry.expression] = entry.portrait;
        }
        isDictInitialized = true;
    }
    
    public Sprite GetPortraitByExpression(Expression expression)
    {
        InitializeDictionary();
        return portraitDict.TryGetValue(expression, out Sprite portrait) 
            ? portrait 
            : portraitDict.GetValueOrDefault(Expression.Default);
    }
}